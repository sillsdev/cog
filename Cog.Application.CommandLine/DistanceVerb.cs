using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;
using SIL.Machine.Annotations;
using SIL.Machine.SequenceAlignment;

namespace SIL.Cog.Application.CommandLine
{
	[Verb("distance", HelpText = "Distance between words")]
	public class DistanceVerb : CommonOptions
	{
		[Value(0, Default = "Aline", HelpText = "Process name (case-insensitive: e.g., Aline or aline)", MetaName = "method")]
		public string Method { get; set; }

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

		public override ReturnCodes DoWork(TextReader inputReader, TextWriter outputWriter, TextWriter errorWriter)
		{
			ReturnCodes retcode = ReturnCodes.Okay;
			SetUpProject();

			WordAlignerBase wordAligner;
			switch (Method.ToLower())
			{
				case "aline":
					wordAligner = (Aline)_project.WordAligners["primary"];
					break;

				default:
					retcode = ReturnCodes.InputError;
					return retcode;
			}

			foreach (string line in inputReader.ReadLines())
			{
				string[] wordTexts = line.Split(' ');
				Word[] words = wordTexts.Select(wordText => ParseWordOnce(wordText, _meaning, _project)).ToArray();
				if (words.Length < 2)
					continue;
				var result = wordAligner.Compute(words[0], words[1]);
				foreach (Alignment<Word, ShapeNode> alignment in result.GetAlignments())
				{
					outputWriter.Write(alignment.ToString(Enumerable.Empty<string>()));
					outputWriter.WriteLine(alignment.RawScore); // Could use alignment.NormalizedScore instead if that would be more useful
					outputWriter.WriteLine();
				}
			}

			return retcode;
		}
	}
}
