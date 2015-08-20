using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;
using SIL.Machine.Annotations;

namespace SIL.Cog.Application.CommandLine
{
	[Verb("distance", HelpText = "Distance between words")]
	public class DistanceVerb : CommonOptions
	{
		[Value(0, Default = "Aline", HelpText = "Process name (case-insensitive: e.g., Aline or aline)", MetaName = "method")]
		public string Method { get; set; }

		private readonly SpanFactory<ShapeNode> _spanFactory = new ShapeSpanFactory();
		private Dictionary<string, Word> _parsedWords = new Dictionary<string, Word>();

		protected Word ParseWordOnce(string wordText, Meaning meaning, CogProject project)
		{
			// We expect to see a lot of duplicates in our input text; save time by memoizing
			if (_parsedWords.ContainsKey(wordText))
				return _parsedWords[wordText];
			Word word = ParseWord(wordText, meaning);
			project.Segmenter.Segment(word);
			_parsedWords.Add(wordText, word);
			return word;
		}

		public override int DoWork(TextReader input, TextWriter output)
		{
			int retcode = (int)ReturnCodes.Okay;
			var segmentPool = new SegmentPool();

			var variety = new Variety("variety1");
			var meaning = new Meaning("gloss1", "cat1");
			CogProject project = CommandLineHelpers.GetProject(_spanFactory, segmentPool);
			project.Meanings.Add(meaning);
			project.Varieties.Add(variety);

			WordAlignerBase wordAligner;
			switch (Method.ToLower())
			{
				case "aline":
					wordAligner = (Aline)project.WordAligners["primary"];
					break;

				default:
					retcode = (int)ReturnCodes.InputError;
					return retcode;
			}

			foreach (string line in input.ReadLines())
			{
				string[] wordTexts = line.Split(' ');
				Word[] words = wordTexts.Select(wordText => ParseWordOnce(wordText, meaning, project)).ToArray();
				var result = wordAligner.Compute(words[0], words[1]);
				string[] outWords = words.Select(word => word.ToString().Replace(" ", "")).ToArray();
				output.WriteLine("{0} {1} {2}", outWords[0], outWords[1], result.BestRawScore);
			}

			return retcode;
		}
	}
}
