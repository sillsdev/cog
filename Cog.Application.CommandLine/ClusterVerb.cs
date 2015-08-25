using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CommandLine;
using QuickGraph.Algorithms;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;
using SIL.Collections;
using SIL.Machine.Annotations;
using SIL.Machine.Clusterers;

namespace SIL.Cog.Application.CommandLine
{
	[Verb("cluster", HelpText = "Cluster words")]
	public class ClusterVerb : CommonOptions
	{
		[Option('t', "threshhold", Default = 0.2, HelpText = "Distance threshhold for a cluster (between 0.0 and 1.0, higher for easier clustering)")]
		public double Threshhold { get; set; }

		public override int DoWork(TextReader input, TextWriter output)
		{
			int retcode = (int)ReturnCodes.Okay;
			SetUpProject();

			var blair = _project.CognateIdentifiers["primary"] as BlairCognateIdentifier;
			var wordAligner = _project.WordAligners["primary"] as Aline;

			var distances = new Dictionary<UnorderedTuple<string, string>, double>();
			var allWords = new HashSet<string>();

			foreach (string line in input.ReadLines())
			{
				string[] words = line.Split(' '); // Format: word1 word2 score (where score is a floating-point number with 1.0 = 100% similarity)
				if (words.Length < 3)
					continue;
				double score = 0;
				if (!Double.TryParse(words[2], out score))
					continue; // TODO: Present meaningful error messages instead of skipping lines
				distances.Add(new UnorderedTuple<string, string>(words[0], words[1]), 1.0 - score);
				allWords.Add(words[0]);
				allWords.Add(words[1]);
			}
			var clusterer = new FlatUpgmaClusterer<string>((w1, w2) => distances[new UnorderedTuple<string, string>(w1, w2)], Threshhold);
			//var clusterer = new NeighborJoiningClusterer<Word>((w1, w2) => distances[new UnorderedTuple<Word, Word>(w1, w2)]);
			IEnumerable<Cluster<string>> clusters = clusterer.GenerateClusters(allWords);
			PrintResults(output, clusters);
			return retcode;
		}

		private static void PrintResults(TextWriter output, IEnumerable<Cluster<string>> clusters)
		{
			int groupnum = 0;
			foreach (var item in clusters)
			{
				groupnum++;
				output.WriteLine("{0} {1}", groupnum, String.Join(" ", item.DataObjects));
			}
		}
	}
}
