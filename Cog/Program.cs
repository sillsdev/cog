using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SIL.Machine;

namespace SIL.Cog
{
	class Program
	{
		public static int Main(string[] args)
		{
			var spanFactory = new ShapeSpanFactory();
			var config = new CogConfig(spanFactory, "data\\ipa-aline.xml");
			config.Load();

			Console.Write("Loading the wordlist...");
			List<Variety> varieties = ReadWordlists(args[0], config.Segmenter).ToList();
			Console.WriteLine("Done.");
			if (varieties.Count == 0)
			{
				Console.WriteLine("The specified file contains no data.");
				return -1;
			}

			var aline = new Aline(spanFactory, config.RelevantVowelFeatures, config.RelevantConsonantFeatures);

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

			string method = args[1].ToLowerInvariant();

			IList<IAnalyzer> analyzers;
			switch (method)
			{
				case "aline":
					analyzers = new IAnalyzer[]
			                	{
			                		new EMSoundChangeInducer(aline, soundChangeAline, 0.5),
			                		new ThresholdCognateIdentifier(soundChangeAline, 0.75)
			                	};
					break;

				case "blair":
					analyzers = new IAnalyzer[]
			                	{
			                		new EMSoundChangeInducer(aline, soundChangeAline, 0.5),
									new BlairCognateIdentifier(soundChangeAline, 0.5) 
			                	};
					break;

				default:
					Console.WriteLine("Invalid method.");
					return -1;
			}

			//var vp = new VarietyPair(varieties[0], varieties[1]);
			//foreach (IAnalyzer analyzer in analyzers)
			//    analyzer.Analyze(vp);
			//using (var writer = new StreamWriter("data\\output.txt"))
			//{
			//    var sb = new StringBuilder();
			//    foreach (SoundChange change in vp.SoundChanges.Where(change => change.ObservedCorrespondenceCount > 0).OrderBy(change => change.Target))
			//    {
			//        if (change.LeftEnvironment != null && change.RightEnvironment != null)
			//            sb.AppendFormat("{0} -> ? / {1} _ {2}", change.Target, change.LeftEnvironment, change.RightEnvironment);
			//        else if (change.LeftEnvironment == null && change.RightEnvironment == null)
			//            sb.AppendFormat("{0} -> ?", change.Target);
			//        else if (change.LeftEnvironment == null)
			//            sb.AppendFormat("{0} -> ? / _ {1}", change.Target, change.RightEnvironment);
			//        else
			//            sb.AppendFormat("{0} -> ? / {1} _", change.Target, change.LeftEnvironment);

			//        sb.AppendLine();
			//        sb.AppendLine("Phoneme\tProb");
			//        foreach (Tuple<string, double> correspondence in change.ObservedCorrespondences.Select(corr => Tuple.Create(corr, change[corr])).OrderByDescending(corr => corr.Item2))
			//        {
			//            sb.AppendFormat("{0}\t{1:0.0####}", correspondence.Item1, correspondence.Item2);
			//            sb.AppendLine();
			//        }
			//        sb.AppendLine();
			//    }
			//    writer.WriteLine(sb);
			//}

			Console.Write("Analyzing Varieties...");
			var forker = new Forker();
			for (int i = 0; i < varieties.Count; i++)
			{
				for (int j = i + 1; j < varieties.Count; j++)
				{
					var varietyPair = new VarietyPair(varieties[i], varieties[j]);
					varieties[i].AddVarietyPair(varietyPair);
					varieties[j].AddVarietyPair(varietyPair);
					forker.Fork(() =>
	                             	{
	                             		foreach (IAnalyzer analyzer in analyzers)
	                             			analyzer.Analyze(varietyPair);
	                             	});
				}
			}
			forker.Join();
			Console.WriteLine("Done.");

			WriteSimilarityGraph(varieties, string.Format("data\\{0}-varieties.dot", method), 0.7);

			Clusterer<Variety> clusterer = new DbscanClusterer<Variety>(variety => variety.VarietyPairs.Where(pair => pair.LexicalSimilarityScore >= 0.7)
					.Select(pair => variety == pair.Variety1 ? pair.Variety2 : pair.Variety1).Concat(variety), 2);
			int clusterID = 1;
			Console.WriteLine("DBSCAN Results:");
			foreach (Cluster<Variety> cluster in clusterer.GenerateClusters(varieties))
			{
				if (cluster.Noise)
					continue;

				Console.WriteLine("Cluster {0}:", clusterID);
				foreach (Variety variety in cluster.DataObjects)
					Console.WriteLine(variety.ID);
				Console.WriteLine();
				clusterID++;
			}
			Console.WriteLine();

			clusterer = new LsdbcClusterer<Variety>(2,
				variety => variety.VarietyPairs.OrderByDescending(pair => pair.LexicalSimilarityScore)
					.Take(3).Select(pair => Tuple.Create(variety == pair.Variety1 ? pair.Variety2 : pair.Variety1, 1.0 - pair.LexicalSimilarityScore)));
			clusterID = 1;
			Console.WriteLine("LSDBC Results:");
			foreach (Cluster<Variety> cluster in clusterer.GenerateClusters(varieties))
			{
				if (cluster.Noise)
					continue;

				Console.WriteLine("Cluster {0}:", clusterID);
				foreach (Variety variety in cluster.DataObjects)
					Console.WriteLine(variety.ID);
				Console.WriteLine();
				clusterID++;
			}
			Console.WriteLine();

			clusterer = new DbscanClusterer<Variety>(variety =>
			                                          	{
			                                          		IEnumerable<Variety> knn = GetKNearestNeighbors(variety, 4);
															return varieties.Where(v => v != variety && knn.Intersect(GetKNearestNeighbors(v, 4)).Count() >= 3).Concat(variety);
			                                          	}, 2);
			clusterID = 1;
			Console.WriteLine("SNN Results:");
			foreach (Cluster<Variety> cluster in clusterer.GenerateClusters(varieties))
			{
				if (cluster.Noise)
					continue;

				Console.WriteLine("Cluster {0}:", clusterID);
				foreach (Variety variety in cluster.DataObjects)
					Console.WriteLine(variety.ID);
				Console.WriteLine();
				clusterID++;
			}
			Console.WriteLine();

			var optics = new Optics<Variety>(variety => variety.VarietyPairs.Select(pair =>
				Tuple.Create(variety == pair.Variety1 ? pair.Variety2 : pair.Variety1, 1.0 - pair.LexicalSimilarityScore)).Concat(Tuple.Create(variety, 0.0)), 2);
			var opticsClusterer = new OpticsDropDownClusterer<Variety>(optics);
			var dbscanClusterer = new DbscanOpticsClusterer<Variety>(optics, 0.3);
			IList<ClusterOrderEntry<Variety>> clusterOrder = opticsClusterer.Optics.ClusterOrder(varieties);
			WriteClusterGraph(dbscanClusterer.GenerateClusters(clusterOrder), opticsClusterer.GenerateClusters(clusterOrder), string.Format("data\\{0}-clusters.dot", method));
			WriteSimilarityMatrix(clusterOrder.Select(oe => oe.DataObject), string.Format("data\\{0}-sim-matrix.txt", method));
			WriteReachabilityPlot(clusterOrder, string.Format("data\\{0}-reachability.txt", method));


			//var writer = new StreamWriter(args[3]);
			//var sb = new StringBuilder();
			//foreach (SoundChange change in pair.SoundChanges.Where(change => change.TotalCorrespondenceCount > 0).OrderBy(change => change.Target))
			//{
			//    if (change.LeftEnvironment != null && change.RightEnvironment != null)
			//        sb.AppendFormat("{0} -> ? / {1} _ {2}", change.Target, change.LeftEnvironment, change.RightEnvironment);
			//    else if (change.LeftEnvironment == null && change.RightEnvironment == null)
			//        sb.AppendFormat("{0} -> ?", change.Target);
			//    else if (change.LeftEnvironment == null)
			//        sb.AppendFormat("{0} -> ? / _ {1}", change.Target, change.RightEnvironment);
			//    else
			//        sb.AppendFormat("{0} -> ? / {1} _", change.Target, change.LeftEnvironment);
				
			//    sb.AppendLine();
			//    sb.AppendLine("Phoneme\tProb\tLink Count");
			//    foreach (Correspondence correspondence in change.Correspondences.Where(corr => corr.Count > 0).OrderByDescending(corr => corr.Probability))
			//    {
			//        sb.AppendFormat("{0}\t{1:0.0####}\t{2}", correspondence.Phoneme, correspondence.Probability, correspondence.Count);
			//        sb.AppendLine();
			//    }
			//    sb.AppendLine();
			//}
			//writer.WriteLine(sb);

			//double totalScore = 0.0;
			//int totalWordCount = 0;
			//var cognates = new List<Tuple<Alignment, string>>();
			//foreach (Tuple<Word, Word> wordPair in pair.WordPairs)
			//{
			//    var aline = new Aline(config, pair, wordPair.Item1.Shape, wordPair.Item2.Shape);
			//    Alignment alignment = aline.GetAlignments().First();
			//    totalScore += alignment.Score;
			//    if (alignment.Score >= 0.75)
			//        cognates.Add(Tuple.Create(alignment, wordPair.Item1.Gloss));
			//    totalWordCount++;
			//}

			//foreach (Tuple<Alignment, string> cognate in cognates.OrderByDescending(cognate => cognate.Item1.Score))
			//{
			//    writer.WriteLine(cognate.Item2);
			//    writer.Write(cognate.Item1.ToString());
			//    writer.WriteLine("Score: {0}", cognate.Item1.Score);
			//    writer.WriteLine();
			//}

			//writer.WriteLine("Lexical Similarity: {0}", (double) cognates.Count / totalWordCount);
			//writer.WriteLine("Avg. Similarity Score: {0}", totalScore / totalWordCount);

			//writer.Close();

			return 0;
		}

		private static IEnumerable<Variety> GetKNearestNeighbors(Variety variety, int k)
		{
			return variety.VarietyPairs.OrderByDescending(pair => pair.LexicalSimilarityScore)
				.Take(k).Select(pair => variety == pair.Variety1 ? pair.Variety2 : pair.Variety1);
		}

		private static IEnumerable<Variety> ReadWordlists(string wordFilePath, Segmenter segmenter)
		{
			using (var file = new StreamReader(wordFilePath))
			{
				string line = file.ReadLine();
				if (line == null)
					return Enumerable.Empty<Variety>();

				string[] varietyIDs = line.Split('\t');
				Dictionary<string, List<Word>> words = varietyIDs.Skip(1).ToDictionary(id => id.Replace("\"", ""), id => new List<Word>());
				while ((line = file.ReadLine()) != null)
				{
					string[] gloss = line.Split('\t');
					for (int i = 1; i < gloss.Length; i++)
					{
						if (!string.IsNullOrEmpty(gloss[i]))
						{
							Shape shape;
							if (segmenter.ToShape(gloss[i], out shape))
								words[varietyIDs[i].Replace("\"", "")].Add(new Word(shape, gloss[0]));
						}
					}
				}

				return words.Select(kvp => new Variety(kvp.Key, kvp.Value));
			}
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
				for (int i = 0; i < varietyArray.Length; i++)
				{
					for (int j = i + 1; j < varietyArray.Length; j++)
					{
						VarietyPair varietyPair = varietyArray[i].VarietyPairs.Single(pair => pair.Variety1 == varietyArray[j] || pair.Variety2 == varietyArray[j]);
						if (varietyPair.LexicalSimilarityScore >= threshold)
							writer.WriteLine("  \"{0}\" -- \"{1}\"", varietyArray[i].ID, varietyArray[j].ID);
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
				var sb = new StringBuilder();
				sb.AppendFormat("  subgraph cluster{0} {{", flatCluster.ID);
				sb.AppendLine();
				foreach (Variety v in flatCluster.DataObjects)
				{
					sb.AppendFormat("    \"{0}\"", v.ID);
					sb.AppendLine();
				}
				subgraphs.Add(Tuple.Create(flatCluster, sb));
			}
			var maingraph = new StringBuilder();
			foreach (Cluster<Variety> cluster in treeClusters)
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
					sb.AppendFormat("{0}\"{1}\" [shape=\"point\", color=\"black\"];", spaces, c.ID);
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

		private static void WriteSimilarityGraph(IEnumerable<ShapeNode> phonemes, string filePath, int threshold, Aline aline)
		{
			ShapeNode[] phonemeArray = phonemes.ToArray();
			using (var writer = new StreamWriter(filePath))
			{
				writer.WriteLine("graph G {");
				writer.WriteLine("  graph [overlap=\"scale\", splines=\"true\"];");
				for (int i = 0; i < phonemeArray.Length; i++)
				{
					var iStrRep = (string) phonemeArray[i].Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep);
					writer.WriteLine("  \"{0}\" [shape=\"circle\"];", iStrRep);
					for (int j = i + 1; j < phonemeArray.Length; j++)
					{
						var jStrRep = (string) phonemeArray[j].Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep);
						if (aline.Delta(phonemeArray[i], phonemeArray[j]) <= threshold)
							writer.WriteLine("  \"{0}\" -- \"{1}\"", iStrRep, jStrRep);
					}
				}

				writer.WriteLine("}");
			}
		}
	}
}
