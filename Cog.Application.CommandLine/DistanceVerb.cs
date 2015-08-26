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

		[Option('r', "raw-scores", Default = false, HelpText = "Produce raw similarity scores (integers from 0 to infinity, where higher means more similar)")]
		public bool RawScores { get; set; }

		[Option('n', "normalized-scores", Default = false, HelpText = "Produce normalized similarity scores (real numbers between 0.0 and 1.0, where higher means more similar)")]
		public bool NormalizedScores { get; set; }

		private Dictionary<string, Word> _parsedWords = new Dictionary<string, Word>();

		Errors errors = new Errors();
		Errors warnings = new Errors();

		protected Word ParseWordOnce(string wordText, Meaning meaning, CogProject project)
		{
			// We expect to see a lot of duplicates in our input text; save time by memoizing
			if (_parsedWords.ContainsKey(wordText))
				return _parsedWords[wordText];
			Word word;
			try
			{
				word = ParseWord(wordText, meaning);
			}
			catch (FormatException e)
			{
				errors.Add(e.Message);
				return null;
			}
			project.Segmenter.Segment(word);
			_parsedWords.Add(wordText, word);
			return word;
		}

		public override ReturnCodes DoWork(TextReader inputReader, TextWriter outputWriter, TextWriter errorWriter)
		{
			ReturnCodes retcode = ReturnCodes.Okay;

			if (!RawScores && !NormalizedScores)
			{
				warnings.Add("Neither raw scores nor normalized scores were selected. Defaulting to normalized.");
				RawScores = false;
				NormalizedScores = true;
			}
			if (RawScores && NormalizedScores)
			{
				warnings.Add("Please specify either raw or normalized scores, but not both. Defaulting to normalized.");
				RawScores = false;
				NormalizedScores = true;
			}

			SetUpProject();

			WordAlignerBase wordAligner;
			switch (Method.ToLower())
			{
				case "aline":
					wordAligner = (Aline)_project.WordAligners["primary"];
					break;

				default:
					warnings.Add("Unknown word aligner \"{0}\". Defaulting to Aline.", Method);
					wordAligner = (Aline)_project.WordAligners["primary"];
					break;
			}

			foreach (string line in inputReader.ReadLines())
			{
				string[] wordTexts = line.Split(' ');
				if (wordTexts.Length != 2)
				{
					errors.Add(line, "Each line should have two space-separated words in it.");
					continue;
				}
				Word[] words = wordTexts.Select(wordText => ParseWordOnce(wordText, _meaning, _project)).ToArray();
				if (words.Length != 2 || words.Any(w => w == null))
				{
					errors.Add(line, "One or more of this line's words failed to parse. Successfully parsed words: {0}", String.Join(", ", words.Where(w => w != null).Select(w => w.StrRep)));
					continue;
				}
				var result = wordAligner.Compute(words[0], words[1]);
				Alignment<Word, ShapeNode> alignment = result.GetAlignments().First();
				outputWriter.Write(alignment.ToString(Enumerable.Empty<string>()));
				outputWriter.WriteLine(RawScores ? alignment.RawScore : alignment.NormalizedScore);
				outputWriter.WriteLine();
			}
			if (!warnings.Empty)
			{
				warnings.Write(errorWriter);
				// Do not set retcode to non-0 if there were only warnings
			}
			if (!errors.Empty)
			{
				errors.Write(errorWriter);
				retcode = ReturnCodes.InputError;
			}


			return retcode;
		}
	}
}
