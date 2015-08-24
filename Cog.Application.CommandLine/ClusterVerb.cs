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

		private Dictionary<string, Word> _parsedWords = new Dictionary<string, Word>();
		protected Word ParseWordOnce(string wordText, Meaning meaning, CogProject project)
		{
			// We expect to see a lot of duplicates in our input text; save time by memoizing
			// TODO: Refactor this with DistanceVerb; do we really need this duplicated code? I think not.
			if (_parsedWords.ContainsKey(wordText))
				return _parsedWords[wordText];
			Word word = ParseWord(wordText, meaning);
			project.Segmenter.Segment(word);
			_parsedWords.Add(wordText, word);
			return word;
		}

		public override int DoWork(TextReader input, TextWriter output) // Version that used Word objects instead of strings... we might be able to just use strings.
		{
			int retcode = (int)ReturnCodes.Okay;
			SetUpProject();

			var blair = _project.CognateIdentifiers["primary"] as BlairCognateIdentifier;
			var wordAligner = _project.WordAligners["primary"] as Aline;

			var distances = new Dictionary<UnorderedTuple<Word, Word>, double>();
			var allWords = new HashSet<Word>();
			
			foreach (string line in input.ReadLines())
			{
				string[] wordTexts = line.Split(' '); // Format: word1 word2 score (where score is a floating-point number with 1.0 = 100% similarity)
				if (wordTexts.Length < 3)
					continue;
				double score = 0;
				if (!Double.TryParse(wordTexts[2], out score))
					continue; // TODO: Present meaningful error messages instead of skipping lines
				Word[] words = wordTexts.Take(2).Select(wordText => ParseWordOnce(wordText, _meaning, _project)).ToArray();
				distances.Add(new UnorderedTuple<Word, Word>(words[0], words[1]), 1.0 - score);
				allWords.Add(words[0]);
				allWords.Add(words[1]);
			}
			var clusterer = new FlatUpgmaClusterer<Word>((w1, w2) => distances[new UnorderedTuple<Word, Word>(w1, w2)], Threshhold);
			//var clusterer = new NeighborJoiningClusterer<Word>((w1, w2) => distances[new UnorderedTuple<Word, Word>(w1, w2)]);
			int groupnum = 0;
			var result = clusterer.GenerateClusters(allWords);
			foreach (var item in result)
			{
				groupnum++;
				output.WriteLine("{0} {1}", groupnum, String.Join(" ", item.DataObjects));
			}
			return retcode;
		}
	}
}
