using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using SIL.Cog.Domain;
using SIL.Machine.Annotations;
using SIL.Machine.SequenceAlignment;

namespace SIL.Cog.CommandLine
{
	[Verb("alignment", HelpText = "Alignment between words")]
	public class AlignmentVerb : VerbBase
	{
		[Option('r', "raw-scores", Default = false,
			HelpText = "Produce raw alignment scores (integers from 0 to infinity, where higher means better-aligned)")]
		public bool RawScores { get; set; }

		[Option('n', "normalized-scores", Default = false,
			HelpText = "Produce normalized alignment scores (real numbers between 0.0 and 1.0, where higher means better-aligned)")]
		public bool NormalizedScores { get; set; }

		[Option('v', "verbose", Default = false,
			HelpText = "Produce more verbose output, showing possible alignments (changes output format)")]
		public bool Verbose { get; set; }

		private readonly Dictionary<string, Word> _parsedWords = new Dictionary<string, Word>();

		protected Word ParseWordOnce(string wordText, Meaning meaning, CogProject project)
		{
			Word word;
			// We expect to see a lot of duplicates in our input text; save time by memoizing
			if (_parsedWords.TryGetValue(wordText, out word))
				return word;
			try
			{
				word = ParseWord(wordText, meaning);
			}
			catch (FormatException e)
			{
				Errors.Add(e.Message);
				return null;
			}
			project.Segmenter.Segment(word);
			_parsedWords.Add(wordText, word);
			return word;
		}

		protected override ReturnCode DoWork(TextReader inputReader, TextWriter outputWriter, TextWriter errorWriter)
		{
			ReturnCode retcode = ReturnCode.Okay;

			if (!RawScores && !NormalizedScores)
			{
				Warnings.Add("Neither raw scores nor normalized scores were selected. Defaulting to normalized.");
				RawScores = false;
				NormalizedScores = true;
			}
			if (RawScores && NormalizedScores)
			{
				Warnings.Add("Please specify either raw or normalized scores, but not both. Defaulting to normalized.");
				RawScores = false;
				NormalizedScores = true;
			}

			SetupProject();
			Meaning meaning = MeaningFactory.Create();

			IWordAligner wordAligner = Project.WordAligners["primary"];
			foreach (string line in ReadLines(inputReader))
			{
				string[] wordTexts = line.Split(' ');
				if (wordTexts.Length != 2)
				{
					Errors.Add(line, "Each line should have two space-separated words in it.");
					continue;
				}
				Word[] words = wordTexts.Select(wordText => ParseWordOnce(wordText, meaning, Project)).ToArray();
				if (words.Length != 2 || words.Any(w => w == null))
				{
					Errors.Add(line, "One or more of this line's words failed to parse. Successfully parsed words: {0}",
						string.Join(", ", words.Where(w => w != null).Select(w => w.StrRep)));
					continue;
				}
				IWordAlignerResult result = wordAligner.Compute(words[0], words[1]);
				Alignment<Word, ShapeNode> alignment = result.GetAlignments().First();
				outputWriter.WriteLine("{0} {1} {2}", words[0].StrRep, words[1].StrRep,
					RawScores ? alignment.RawScore : alignment.NormalizedScore);
				if (Verbose)
				{
					outputWriter.Write(alignment.ToString(Enumerable.Empty<string>()));
					outputWriter.WriteLine();
				}
			}

			return retcode;
		}
	}
}
