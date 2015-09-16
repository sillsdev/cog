using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CommandLine;
using SIL.Collections;
using SIL.Machine.Clusterers;

namespace SIL.Cog.CommandLine
{
	[Verb("cluster", HelpText = "Cluster words")]
	public class ClusterVerb : VerbBase
	{
		[Value(0, Default = "upgma", HelpText = "Process name (case-insensitive); valid values are \"upgma\", \"dbscan\", and \"lsdbc\"", MetaName = "method")]
		public string Method { get; set; }

		[Option('t', "threshhold", Default = 0.2, HelpText = "Distance threshhold for UPGMA or DBSCAN clustering (between 0.0 and 1.0, higher for easier clustering)")]
		public double Threshhold { get; set; }

		[Option('m', "min-points", Default = 2, HelpText = "Minimum # of points to form a cluster in DBSCAN clustering")]
		public int MinPoints { get; set; }

		[Option('a', "alpha", Default = 4, HelpText = "Alpha value for LSDBC clustering (weight factor for forming new clusters)")]
		public double Alpha { get; set; }

		[Option('k', Default = 3, HelpText = "How many neighbors to consider in LSDBC clustering (value of K for the K-nearest-neighbors algorithm)")]
		public int K { get; set; }

		Dictionary<UnorderedTuple<string, string>, double> distanceDict = new Dictionary<UnorderedTuple<string, string>, double>();
		Dictionary<string, List<Tuple<double, string>>> distanceGraph = new Dictionary<string, List<Tuple<double, string>>>();
		HashSet<string> allWords = new HashSet<string>();

		protected override ReturnCodes DoWork(TextReader inputReader, TextWriter outputWriter, TextWriter errorWriter)
		{
			ReturnCodes retcode = ReturnCodes.Okay;
			SetUpProject();

			string lowerMethod = Method.ToLowerInvariant();
			switch (lowerMethod)
			{
				case "dbscan":
				case "lsdbc":
				case "upgma":
					break;
				default:
					Errors.Add(string.Format("Invalid clustering method {0}. Valid values are \"upgma\", \"dbscan\", and \"lsdbc\" (not case-sensitive, e.g. \"Upgma\" also works.)", Method));
					return ReturnCodes.InputError;
			}

			foreach (string line in ReadLines(inputReader))
			{
				string[] words = line.Split(' '); // Format: word1 word2 score (where score is a floating-point number with 1.0 = 100% similarity)
				if (words.Length < 3)
				{
					Errors.Add(line, "Each line should contain two words and one score, separated by spaces.");
					continue;
				}
				double score = -1;
				if (!Double.TryParse(words[2], NumberStyles.Float, CultureInfo.InvariantCulture, out score))
				{
					Errors.Add(line, "Could not parse score \"{0}\". Scores should be a number between 0 and 1.", words[2]);
					continue;
				}
				if (score < 0.0)
				{
					Errors.Add(line, "Invalid score \"{0}\". Scores should not be negative, but should be a number between 0 and 1.", words[2]);
					continue;
				}
				if (score > 1.0)
				{
					Errors.Add(line, "Invalid score \"{0}\". Scores should not be greater than 1, but should be a number between 0 and 1.", words[2]);
					continue;
				}

				double distance = 1.0 - score;
				KeepScoreForUpgmaClusterer(words[0], words[1], distance); // TODO: Since we always call this, maybe we should rename it
			}
			IFlatClusterer<string> clusterer;
			switch (Method.ToLowerInvariant())
			{

				case "dbscan":
					// public DbscanClusterer(Func<T, IEnumerable<T>> getNeighbors, double minPoints)
					KeepScoreForDbscanClusterer();
					clusterer = new DbscanClusterer<string>(word => distanceGraph[word].TakeWhile(scoreWordTuple => scoreWordTuple.Item1 <= Threshhold).Select(scoreWordTuple => scoreWordTuple.Item2), MinPoints);
					break;

				case "lsdbc":
					// public LsdbcClusterer(double alpha, Func<T, IEnumerable<Tuple<T, double>>> getKNearestNeighbors)
					KeepScoreForLsdbcClusterer();
					clusterer = new LsdbcClusterer<string>(Alpha, word => distanceGraph[word].Take(K).Select(tuple => new Tuple<string, double>(tuple.Item2, tuple.Item1)));
					break;

				case "upgma":
				default:
					clusterer = new FlatUpgmaClusterer<string>((w1, w2) => distanceDict[new UnorderedTuple<string, string>(w1, w2)], Threshhold);
					break;
			}
			IEnumerable<Cluster<string>> clusters = clusterer.GenerateClusters(allWords);
			PrintResults(outputWriter, clusters);
			return retcode;
		}

		private void KeepScoreForUpgmaClusterer(string word1, string word2, double distance)
		{
			distanceDict.Add(new UnorderedTuple<string, string>(word1, word2), distance);

			allWords.Add(word1);
			allWords.Add(word2);
		}

		private void KeepScoreForLsdbcClusterer()
		{
			foreach (KeyValuePair<UnorderedTuple<string, string>, double> kv in distanceDict)
			{
				UnorderedTuple<string, string> wordPair = kv.Key;
				double distance = kv.Value;
				AddToListDict(distanceGraph, wordPair.Item1, new Tuple<double, string>(distance, wordPair.Item2));
				AddToListDict(distanceGraph, wordPair.Item2, new Tuple<double, string>(distance, wordPair.Item1));
			}
			foreach (List<Tuple<double, string>> scoreList in distanceGraph.Values)
				scoreList.Sort(); // TODO: If performance is a serious issue, can replace scoreList with a BinaryHeap<> instead of a List<>. Leaving things simple for now.
		}

		private void KeepScoreForDbscanClusterer()
		{
			KeepScoreForLsdbcClusterer();
		}

		private static void AddToListDict<TKey, TValue>(Dictionary<TKey, List<TValue>> dictionary, TKey key, TValue value)
		{
			// Could be an extension method, but it's such a specialized case I don't think an extension method is worth the extra complexity of one more class.
			List<TValue> list;
			if (!dictionary.TryGetValue(key, out list))
			{
				list = new List<TValue>();
				dictionary.Add(key, list);
			}
			list.Add(value);
		}
			
		private static void PrintResults(TextWriter output, IEnumerable<Cluster<string>> clusters)
		{
			int groupnum = 0;
			foreach (Cluster<string> item in clusters)
			{
				if (item.DataObjects.Count == 0)
					continue; // Skip any empty clusters
				groupnum++;
				output.WriteLine("{0} {1}", groupnum, String.Join(" ", item.DataObjects));
			}
		}
	}
}
