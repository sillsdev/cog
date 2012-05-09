using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NDesk.Options;
using SIL.Collections;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog
{
	class Program
	{
		public static int Main(string[] args)
		{
			string configFile = null;
            string inputFile = null;
			string outputFolder = null;
			string format = "text";
			string method = "blair";
            bool showHelp = false;

			var p = new OptionSet
                    	{
							{ "c|config-file=", "read configuration from {FILE}", value => configFile = value },
							{ "f|input-format=", "the format of the input file {[text|wordsurv|comparanda]}, default: text", value => format = value },
                    		{ "i|input-file=", "read word lists from {FILE}", value => inputFile = value },
							{ "o|output-folder=", "saves results to {FOLDER}", value => outputFolder = value },
							{ "m|method=", "comparison method {[blair|aline]}, default: blair", value => method = value },
							{ "h|help", "show this help message and exit", value => showHelp = value != null }
                    	};

            try
            {
                p.Parse(args);
            }
            catch (OptionException)
            {
                ShowHelp(p);
                return -1;
            }

            if (showHelp || string.IsNullOrEmpty(inputFile) || string.IsNullOrEmpty(configFile) || string.IsNullOrEmpty(outputFolder))
            {
                ShowHelp(p);
                return -1;
            }

			var spanFactory = new ShapeSpanFactory();
			var config = new CogConfig(spanFactory, configFile);
			config.Load();

			Console.Write("Loading the wordlist...");

			WordListsLoader loader;
			switch (format)
			{
				case "text":
					loader = new TextLoader(config.Segmenter, inputFile);
					break;
				case "wordsurv":
					loader = new WordSurvXmlLoader(config.Segmenter, inputFile);
					break;
				case "comparanda":
					loader = new ComparandaLoader(config.Segmenter, inputFile);
					break;
				default:
					ShowHelp(p);
					return -1;
			}

			List<Variety> varieties = loader.Load().ToList();
			Console.WriteLine("Done.");
			if (varieties.Count == 0)
			{
				Console.WriteLine("The specified file contains no data.");
				return -1;
			}

			Directory.CreateDirectory(outputFolder);

			IList<IProcessor<Variety>> varietyProcessors;
			IList<IProcessor<VarietyPair>> varietyPairProcessors;
			switch (method)
			{
				case "aline":
					{
						varietyProcessors = new IProcessor<Variety>[]
			                                {
			                                    new UnsupervisedStemmer(spanFactory, 100.0, 5),
			                                    new VarietyTextOutput(outputFolder)
			                                };

						var settings = new EditDistanceSettings {Mode = EditDistanceMode.Local};
						var aline = new Aline(spanFactory, config.RelevantVowelFeatures, config.RelevantConsonantFeatures, settings);
						var soundChangeAline = new SoundChangeAline(spanFactory, config.RelevantVowelFeatures, config.RelevantConsonantFeatures,
							config.NaturalClasses, settings);
						varietyPairProcessors = new IProcessor<VarietyPair>[]
					                        	{
													new WordPairGenerator(aline), 
					                        		new EMSoundChangeInducer(aline, soundChangeAline, 0.5),
					                        		new ThresholdCognateIdentifier(soundChangeAline, 0.5),
					                        		new VarietyPairTextOutput(outputFolder, soundChangeAline)
					                        	};
					}
					break;

				case "blair":
					{
						varietyProcessors = new IProcessor<Variety>[]
			                                {
			                                    new VarietyTextOutput(outputFolder)
			                                };

						var settings = new EditDistanceSettings {DisableExpansionCompression = true};
						var aline = new Aline(spanFactory, config.RelevantVowelFeatures, config.RelevantConsonantFeatures, settings);
						var soundChangeAline = new SoundChangeAline(spanFactory, config.RelevantVowelFeatures, config.RelevantConsonantFeatures,
							config.NaturalClasses, settings);
						varietyPairProcessors = new IProcessor<VarietyPair>[]
			                					{
													new WordPairGenerator(aline), 
			                						new EMSoundChangeInducer(aline, soundChangeAline, 0.25),
													new SimilarSegmentListIdentifier("data\\similar-vowels.txt", "data\\similar-consonants.txt", config.Segmenter.Joiners), 
													new BlairCognateIdentifier(soundChangeAline, 0.25),
													new VarietyPairTextOutput(outputFolder, soundChangeAline) 
			                					};
					}
					break;

				default:
					ShowHelp(p);
					return -1;
			}

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

			WritePhonemeList(varieties, CogFeatureSystem.VowelType, Path.Combine(outputFolder, "vowels.txt"));
			WritePhonemeList(varieties, CogFeatureSystem.ConsonantType, Path.Combine(outputFolder, "consonants.txt"));

			var varietyPairs = new HashSet<VarietyPair>(varieties.SelectMany(variety => variety.VarietyPairs));
			var varietyPairEvent = new CountdownEvent(varietyPairs.Count * varietyPairProcessors.Count);
			Console.Write("Analyzing variety pairs...  0%");

			foreach (VarietyPair varietyPair in varietyPairs)
			{
				VarietyPair vp = varietyPair;
				Task.Factory.StartNew(() =>
					                    {
											foreach (IProcessor<VarietyPair> varietyPairProcessor in varietyPairProcessors)
											{
												varietyPairProcessor.Process(vp);
												varietyPairEvent.Signal();
											}
					                    });
			}

			int lastPcnt = 0;
			while (!varietyPairEvent.Wait(500))
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

			WriteSimilarityGraph(varieties, Path.Combine(outputFolder, "varieties.dot"), 0.7);
			var optics = new Optics<Variety>(variety => variety.VarietyPairs.Select(pair =>
				Tuple.Create(variety == pair.Variety1 ? pair.Variety2 : pair.Variety1, 1.0 - pair.LexicalSimilarityScore)).Concat(Tuple.Create(variety, 0.0)), 2);
			var opticsClusterer = new OpticsDropDownClusterer<Variety>(optics);
			var dbscanClusterer = new DbscanOpticsClusterer<Variety>(optics, 0.3);
			IList<ClusterOrderEntry<Variety>> clusterOrder = opticsClusterer.Optics.ClusterOrder(varieties);
			WriteClusterGraph(dbscanClusterer.GenerateClusters(clusterOrder), opticsClusterer.GenerateClusters(clusterOrder), Path.Combine(outputFolder, "clusters.dot"));
			WriteSimilarityMatrix(clusterOrder.Select(oe => oe.DataObject), Path.Combine(outputFolder, "sim-matrix.txt"));
			WriteReachabilityPlot(clusterOrder, Path.Combine(outputFolder, "reachability.txt"));

			return 0;
		}

        private static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: cog [OPTIONS]");
            Console.WriteLine("Cog is a tool for comparative linguistics.");
            Console.WriteLine();
            p.WriteOptionDescriptions(Console.Out);
        }

		private static IEnumerable<Variety> GetKNearestNeighbors(Variety variety, int k)
		{
			return variety.VarietyPairs.OrderByDescending(pair => pair.LexicalSimilarityScore)
				.Take(k).Select(pair => variety == pair.Variety1 ? pair.Variety2 : pair.Variety1);
		}

		private static void WriteSimilarityMatrix(IEnumerable<Variety> varieties, string filePath)
		{
			Variety[] varietyArray = varieties.ToArray();
			using (var writer = new StreamWriter(filePath))
			{
				for (int i = 0; i < varietyArray.Length; i++)
				{
					for (int j = 0; j < i; j++)
					{
						if (j > 0)
							writer.Write("\t");
						VarietyPair varietyPair = varietyArray[i].VarietyPairs.Single(pair => pair.Variety1 == varietyArray[j] || pair.Variety2 == varietyArray[j]);
						writer.Write("{0:0.00}", varietyPair.LexicalSimilarityScore);
					}
					if (i > 0)
						writer.Write("\t");
					writer.WriteLine(varietyArray[i].ID);
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
