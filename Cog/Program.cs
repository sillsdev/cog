using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog
{
	class Program
	{
		public static int Main(string[] args)
		{
			string name = Path.GetFileNameWithoutExtension(args[0]);
			string method = args[1].ToLowerInvariant();

			var spanFactory = new ShapeSpanFactory();
			var config = new CogConfig(spanFactory, "data\\ipa-aline.xml");
			config.Load();

			Console.Write("Loading the wordlist...");
			List<Variety> varieties = ReadWordlists(args[0], config.Segmenter).ToList();
			//Dictionary<Tuple<string, string>, HashSet<string>> cognates;
			//List<Variety> varieties = ReadComparanda(args[0], config.Segmenter, out cognates).ToList();
			Console.WriteLine("Done.");
			if (varieties.Count == 0)
			{
				Console.WriteLine("The specified file contains no data.");
				return -1;
			}

			string dir = Path.Combine("data", string.Format("{0}-{1}", name, method));
			Directory.CreateDirectory(dir);

			IList<IProcessor<Variety>> varietyProcessors = new IProcessor<Variety>[]
			                                               	{
			                                               		new UnsupervisedStemmer(spanFactory, 100.0, 5),
			                                               		new VarietyTextOutput(dir)
			                                               	};

			var varietyEvent = new CountdownEvent(varieties.Count * varietyProcessors.Count);
			Console.Write("Analyzing varieties...");
			foreach (Variety variety in varieties)
			{
				Variety v = variety;
				Task.Factory.StartNew(() =>
				{
					foreach (IProcessor<Variety> varietyProcessor in varietyProcessors)
					{
						varietyProcessor.Process(v);
						varietyEvent.Signal();
					}
				});
			}
			varietyEvent.Wait();
			Console.WriteLine("Done.");

			var aline = new Aline(spanFactory, config.RelevantVowelFeatures, config.RelevantConsonantFeatures);

			WritePhonemeList(varieties, CogFeatureSystem.VowelType, Path.Combine(dir, "vowels.txt"));
			WritePhonemeList(varieties, CogFeatureSystem.ConsonantType, Path.Combine(dir, "consonants.txt"));

			//WriteSimilarityGraph((from variety in varieties
			//                      from word in variety.Words
			//                      from node in word.Shape
			//                      where node.Annotation.Type == CogFeatureSystem.VowelType
			//                      select node).DistinctBy(node => (string)node.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep)), "data\\vowels.dot", 500, aline);

			//WriteSimilarityGraph((from variety in varieties
			//                      from word in variety.Words
			//                      from node in word.Shape
			//                      where node.Annotation.Type == CogFeatureSystem.ConsonantType
			//                      select node).DistinctBy(node => (string)node.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep)), "data\\consonants.dot", 500, aline);

			var soundChangeAline = new SoundChangeAline(spanFactory, config.RelevantVowelFeatures, config.RelevantConsonantFeatures, config.NaturalClasses);

			IList<IProcessor<VarietyPair>> varietyPairProcessors;
			switch (method)
			{
				case "aline":
					varietyPairProcessors = new IProcessor<VarietyPair>[]
					                        	{
					                        		new EMSoundChangeInducer(aline, soundChangeAline, 0.5),
					                        		new ThresholdCognateIdentifier(soundChangeAline, 0.5),
					                        		new VarietyPairTextOutput(dir, soundChangeAline)
					                        	};
					break;

				case "blair":
					varietyPairProcessors = new IProcessor<VarietyPair>[]
			                					{
			                						new EMSoundChangeInducer(aline, soundChangeAline, 0.5),
													new SimilarSegmentListIdentifier("data\\similar-segments.txt"), 
													//new SimilarSegmentThresholdIdentifier(soundChangeAline, 500, 600), 
													new BlairCognateIdentifier(soundChangeAline, 0.5),
													new VarietyPairTextOutput(dir, soundChangeAline) 
			                					};
					break;

				default:
					Console.WriteLine("Invalid method.");
					return -1;
			}

			Console.Write("Analyzing variety pairs...  0%");
			var varietyPairEvent = new CountdownEvent(((varieties.Count * (varieties.Count - 1)) / 2) * varietyPairProcessors.Count);
			for (int i = 0; i < varieties.Count; i++)
			{
				for (int j = i + 1; j < varieties.Count; j++)
				{
					var varietyPair = new VarietyPair(varieties[i], varieties[j]);
					varieties[i].AddVarietyPair(varietyPair);
					varieties[j].AddVarietyPair(varietyPair);
					//HashSet<string> cognateGlosses;
					//if (cognates.TryGetValue(Tuple.Create(varietyPair.Variety1.ID, varietyPair.Variety2.ID), out cognateGlosses))
					//{
					//    foreach (WordPair wp in varietyPair.WordPairs)
					//        wp.AreCognatesActual = cognateGlosses.Contains(wp.Word1.Gloss);
					//}
					Task.Factory.StartNew(() =>
					                      	{
												foreach (IProcessor<VarietyPair> varietyPairProcessor in varietyPairProcessors)
												{
													varietyPairProcessor.Process(varietyPair);
													varietyPairEvent.Signal();
												}
					                      	});
				}
			}

			int lastPcnt = 0;
			while (!varietyPairEvent.Wait(2000))
			{
				int curPcnt = ((varietyPairEvent.InitialCount - varietyPairEvent.CurrentCount) * 100) / varietyPairEvent.InitialCount;
				if (curPcnt != lastPcnt)
				{
					Console.Write("\b\b\b\b{0}%", curPcnt.ToString(CultureInfo.InvariantCulture).PadLeft(3));
					lastPcnt = curPcnt;
				}
			}
			Console.WriteLine("\b\b\b\bDone.");

			//Clusterer<Variety> clusterer = new DbscanClusterer<Variety>(variety => variety.VarietyPairs.Where(pair => pair.LexicalSimilarityScore >= 0.7)
			//        .Select(pair => variety == pair.Variety1 ? pair.Variety2 : pair.Variety1).Concat(variety), 2);
			//int clusterID = 1;
			//Console.WriteLine("DBSCAN Results:");
			//foreach (Cluster<Variety> cluster in clusterer.GenerateClusters(varieties))
			//{
			//    if (cluster.Noise)
			//        continue;

			//    Console.WriteLine("Cluster {0}:", clusterID);
			//    foreach (Variety variety in cluster.DataObjects)
			//        Console.WriteLine(variety.ID);
			//    Console.WriteLine();
			//    clusterID++;
			//}
			//Console.WriteLine();

			//clusterer = new LsdbcClusterer<Variety>(2,
			//    variety => variety.VarietyPairs.OrderByDescending(pair => pair.LexicalSimilarityScore)
			//        .Take(3).Select(pair => Tuple.Create(variety == pair.Variety1 ? pair.Variety2 : pair.Variety1, 1.0 - pair.LexicalSimilarityScore)));
			//clusterID = 1;
			//Console.WriteLine("LSDBC Results:");
			//foreach (Cluster<Variety> cluster in clusterer.GenerateClusters(varieties))
			//{
			//    if (cluster.Noise)
			//        continue;

			//    Console.WriteLine("Cluster {0}:", clusterID);
			//    foreach (Variety variety in cluster.DataObjects)
			//        Console.WriteLine(variety.ID);
			//    Console.WriteLine();
			//    clusterID++;
			//}
			//Console.WriteLine();

			//clusterer = new DbscanClusterer<Variety>(variety =>
			//                                            {
			//                                                IEnumerable<Variety> knn = GetKNearestNeighbors(variety, 4);
			//                                                return varieties.Where(v => v != variety && knn.Intersect(GetKNearestNeighbors(v, 4)).Count() >= 3).Concat(variety);
			//                                            }, 2);
			//clusterID = 1;
			//Console.WriteLine("SNN Results:");
			//foreach (Cluster<Variety> cluster in clusterer.GenerateClusters(varieties))
			//{
			//    if (cluster.Noise)
			//        continue;

			//    Console.WriteLine("Cluster {0}:", clusterID);
			//    foreach (Variety variety in cluster.DataObjects)
			//        Console.WriteLine(variety.ID);
			//    Console.WriteLine();
			//    clusterID++;
			//}
			//Console.WriteLine();

			WriteSimilarityGraph(varieties, Path.Combine(dir, "varieties.dot"), 0.7);
			var optics = new Optics<Variety>(variety => variety.VarietyPairs.Select(pair =>
				Tuple.Create(variety == pair.Variety1 ? pair.Variety2 : pair.Variety1, 1.0 - pair.LexicalSimilarityScore)).Concat(Tuple.Create(variety, 0.0)), 2);
			var opticsClusterer = new OpticsDropDownClusterer<Variety>(optics);
			var dbscanClusterer = new DbscanOpticsClusterer<Variety>(optics, 0.3);
			IList<ClusterOrderEntry<Variety>> clusterOrder = opticsClusterer.Optics.ClusterOrder(varieties);
			WriteClusterGraph(dbscanClusterer.GenerateClusters(clusterOrder), opticsClusterer.GenerateClusters(clusterOrder), Path.Combine(dir, "clusters.dot"));
			WriteSimilarityMatrix(clusterOrder.Select(oe => oe.DataObject), Path.Combine(dir, "sim-matrix.txt"));
			WriteReachabilityPlot(clusterOrder, Path.Combine(dir, "reachability.txt"));

			return 0;
		}

		private static IEnumerable<Variety> GetKNearestNeighbors(Variety variety, int k)
		{
			return variety.VarietyPairs.OrderByDescending(pair => pair.LexicalSimilarityScore)
				.Take(k).Select(pair => variety == pair.Variety1 ? pair.Variety2 : pair.Variety1);
		}

		private static IEnumerable<Variety> ReadWordlists(string wordFilePath, Segmenter segmenter)
		{
			var varieties = new List<Variety>();
			using (var file = new StreamReader(wordFilePath))
			{
				string line = file.ReadLine();
				if (line == null)
					return Enumerable.Empty<Variety>();

				string[] glosses = line.Split('\t');

				line = file.ReadLine();
				if (line == null)
					return Enumerable.Empty<Variety>();

				string[] categories = line.Split('\t');
				while ((line = file.ReadLine()) != null)
				{
					var words = new List<Word>();
					string[] wordStrs = line.Split('\t');
					for (int i = 1; i < wordStrs.Length; i++)
					{
						string wordStr = wordStrs[i].Trim();
						if (!string.IsNullOrEmpty(wordStr))
						{
							string w = wordStr.Split(',').First().Trim();
							Shape shape;
							if (segmenter.ToShape(w, out shape))
								words.Add(new Word(shape, glosses[i].Trim(), categories[i].Trim()));
						}
					}

					varieties.Add(new Variety(wordStrs[0].Trim(), words));
				}
			}

			return varieties;
		}

		private static IEnumerable<Variety> ReadComparanda(string path, Segmenter segmenter, out Dictionary<Tuple<string, string>, HashSet<string>> cognates)
		{
			var varietyWords = new Dictionary<string, List<Word>>();
			cognates = new Dictionary<Tuple<string, string>, HashSet<string>>();

			XElement root = XElement.Load(path);
			foreach (XElement concept in root.Elements("CONCEPT"))
			{
				var gloss = (string) concept.Attribute("ID");
				var category = (string) concept.Attribute("ROLE");
				foreach (XElement varietyElem in concept.Elements().Where(elem => elem.Name != "NOTE"))
				{
					string id = varietyElem.Name.LocalName.ToLowerInvariant();
					List<Word> words;
					if (!varietyWords.TryGetValue(id, out words))
					{
						words = new List<Word>();
						varietyWords[id] = words;
					}
					var str = ((string) varietyElem.Element("STEM").Attribute("PHON")).Replace(" ", "");
					Shape shape;
					if (segmenter.ToShape(str, out shape))
						words.Add(new Word(shape, gloss, category));

					var cognateVarieties = (string) varietyElem.Attribute("COGN_PROB");
					if (!string.IsNullOrEmpty(cognateVarieties))
					{
						foreach (string id2 in cognateVarieties.Split(','))
						{
							Tuple<string, string> key = Tuple.Create(id, id2);
							HashSet<string> cogs;
							if (!cognates.TryGetValue(key, out cogs))
							{
								cogs = new HashSet<string>();
								cognates[key] = cogs;
							}
							cogs.Add(gloss);
						}
					}
				}
			}

			return varietyWords.Select(kvp => new Variety(kvp.Key, kvp.Value));
		}

		private static void WriteSimilarityMatrix(IEnumerable<Variety> varieties, string filePath)
		{
			Variety[] varietyArray = varieties.ToArray();
			using (var writer = new StreamWriter(filePath))
			{
				foreach (Variety variety in varietyArray.Reverse())
				{
					writer.Write("\t");
					writer.Write(variety.ID);
				}
				writer.WriteLine();

				for (int i = 0; i < varietyArray.Length; i++)
				{
					writer.Write(varietyArray[i].ID);
					for (int j = varietyArray.Length - 1; j > i; j--)
					{
						VarietyPair varietyPair = varietyArray[i].VarietyPairs.Single(pair => pair.Variety1 == varietyArray[j] || pair.Variety2 == varietyArray[j]);
						writer.Write("\t{0:0.0####}", varietyPair.LexicalSimilarityScore);
					}
					writer.WriteLine();
				}
			}
		}

		private static void WriteSimilarityGraph(IEnumerable<Variety> varieties, string filePath, double threshold)
		{
			Variety[] varietyArray = varieties.ToArray();
			using (var writer = new StreamWriter(filePath))
			{
				writer.WriteLine("graph G {");
				writer.WriteLine("  graph [overlap=\"scale\", splines=\"true\"];");
				writer.WriteLine("  edge [colorscheme=\"reds9\"];");
				for (int i = 0; i < varietyArray.Length; i++)
				{
					writer.WriteLine("  \"{0}\"", varietyArray[i].ID);
					for (int j = i + 1; j < varietyArray.Length; j++)
					{
						VarietyPair varietyPair = varietyArray[i].VarietyPairs.Single(pair => pair.Variety1 == varietyArray[j] || pair.Variety2 == varietyArray[j]);
						if (varietyPair.LexicalSimilarityScore >= threshold)
						{
							int c;
							for (c = 0; c < 7; c++)
							{
								if (varietyPair.LexicalSimilarityScore <= threshold + (((1.0 - threshold) / 7) * (c + 1)))
									break;
							}

							writer.WriteLine("  \"{0}\" -- \"{1}\" [color=\"{2}\"];", varietyArray[i].ID, varietyArray[j].ID, (c + 2).ToString(CultureInfo.InvariantCulture));
						}
					}
				}

				writer.WriteLine("}");
			}
		}

		private static void WriteReachabilityPlot(IList<ClusterOrderEntry<Variety>> clusterOrder, string filePath)
		{
			using (var writer = new StreamWriter(filePath))
			{
				foreach (ClusterOrderEntry<Variety> entry in clusterOrder)
					writer.WriteLine("{0}\t{1:0.0####}", entry.DataObject.ID, entry.Reachability);
			}
		}

		private static void WriteClusterGraph(IEnumerable<Cluster<Variety>> flatClusters, IEnumerable<Cluster<Variety>> treeClusters, string filePath)
		{
			var subgraphs = new List<Tuple<Cluster<Variety>, StringBuilder>>();
			foreach (Cluster<Variety> flatCluster in flatClusters.Where(c => !c.Noise))
			{
				string color = ((subgraphs.Count % 9) + 1).ToString(CultureInfo.InvariantCulture);
				var sb = new StringBuilder();
				sb.AppendFormat("  subgraph cluster{0} {{", flatCluster.ID);
				sb.AppendLine();
				sb.AppendFormat("    graph [colorscheme=\"set19\", color=\"{0}\"];", color);
				sb.AppendLine();
				foreach (Variety v in flatCluster.DataObjects)
				{
					sb.AppendFormat("    \"{0}\" [shape=\"plaintext\", colorscheme=\"set19\" fontcolor=\"{1}\"];", v.ID, color);
					sb.AppendLine();
				}
				subgraphs.Add(Tuple.Create(flatCluster, sb));
			}
			var maingraph = new StringBuilder();
			Cluster<Variety>[] tree = treeClusters.ToArray();

			if (tree.Length > 1)
			{
				Tuple<Cluster<Variety>, StringBuilder> subgraph = subgraphs.SingleOrDefault(sg => sg.Item1.DataObjects.Intersect(tree.SelectMany(c => c.DataObjects)).Count() == tree.Sum(c => c.DataObjectCount));
				StringBuilder sb;
				string spaces;
				if (subgraph != null)
				{
					sb = subgraph.Item2;
					spaces = "    ";
				}
				else
				{
					sb = maingraph;
					spaces = "  ";
				}

				var childVarieties = new HashSet<Variety>();
				sb.AppendFormat("{0}\"root\" [shape=\"box3d\", label=\"\"];", spaces);
				sb.AppendLine();
				foreach (Cluster<Variety> child in tree)
				{
					sb.AppendFormat("{0}\"root\" -> \"{1}\"", spaces, child.ID);
					sb.AppendLine();
					childVarieties.UnionWith(child.DataObjects);
				}

			}


			foreach (Cluster<Variety> cluster in tree)
			{
				foreach (Cluster<Variety> c in cluster.GetNodesBreadthFirst())
				{
					Tuple<Cluster<Variety>, StringBuilder> subgraph = subgraphs.SingleOrDefault(sg => sg.Item1.DataObjects.Intersect(c.DataObjects).Count() == c.DataObjectCount);
					StringBuilder sb;
					string spaces;
					if (subgraph != null)
					{
						sb = subgraph.Item2;
						spaces = "    ";
					}
					else
					{
						sb = maingraph;
						spaces = "  ";
					}

					var childVarieties = new HashSet<Variety>();
					sb.AppendFormat("{0}\"{1}\" [shape=\"{2}\", label=\"\"];", spaces, c.ID, tree.Length == 1 && c.Parent == null ? "box3d" : "point");
					sb.AppendLine();
					foreach (Cluster<Variety> child in c.Children)
					{
						sb.AppendFormat("{0}\"{1}\" -> \"{2}\"", spaces, c.ID, child.ID);
						sb.AppendLine();
						childVarieties.UnionWith(child.DataObjects);
					}

					foreach (Variety variety in c.DataObjects.Except(childVarieties))
					{
						sb.AppendFormat("{0}\"{1}\" -> \"{2}\"", spaces, c.ID, variety.ID);
						sb.AppendLine();
					}
				}
			}

			using (var writer = new StreamWriter(filePath))
			{
				writer.WriteLine("digraph G {");
				writer.WriteLine("  graph [rankdir=\"LR\"];");
				writer.WriteLine("  node [shape=\"plaintext\"];");
				foreach (Tuple<Cluster<Variety>, StringBuilder> subgraph in subgraphs)
				{
					writer.Write(subgraph.Item2.ToString());
					writer.WriteLine("  }");
					writer.WriteLine();
				}
				writer.Write(maingraph.ToString());
				writer.WriteLine("}");
			}
		}

		private static void WriteSimilarityGraph(IEnumerable<Variety> varieties, FeatureSymbol type, string filePath, int threshold, Aline aline)
		{
			Segment[] segmentArray = (from v in varieties
									  from ph in v.Segments
									  where ph.Type == type
									  select ph).DistinctBy(ph => ph.StrRep).ToArray();
			using (var writer = new StreamWriter(filePath))
			{
				writer.WriteLine("graph G {");
				writer.WriteLine("  graph [overlap=\"scale\", splines=\"true\"];");
				for (int i = 0; i < segmentArray.Length; i++)
				{
					string iStrRep = segmentArray[i].StrRep;
					writer.WriteLine("  \"{0}\" [shape=\"circle\"];", iStrRep);
					for (int j = i + 1; j < segmentArray.Length; j++)
					{
						string jStrRep = segmentArray[j].StrRep;
						if (aline.Delta(segmentArray[i].FeatureStruct, segmentArray[j].FeatureStruct) <= threshold)
							writer.WriteLine("  \"{0}\" -- \"{1}\"", iStrRep, jStrRep);
					}
				}

				writer.WriteLine("}");
			}
		}

		private static void WritePhonemeList(IEnumerable<Variety> varieties, FeatureSymbol type, string filePath)
		{
			using (var writer = new StreamWriter(filePath))
			{
				foreach (string phoneme in (from v in varieties
										    from ph in v.Segments
										    where ph.Type == type
										    select ph.StrRep).Distinct())
				{
					writer.WriteLine(phoneme);
				}
			}
		}
	}
}
