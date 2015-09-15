using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;
using SIL.Machine.Annotations;
using SIL.Machine.SequenceAlignment;

namespace SIL.Cog.CommandLine
{
	[Verb("distance", HelpText = "Distance between words")]
	public class DistanceVerb : VerbBase
	{
		[Value(0, Default = "Aline", HelpText = "Process name (case-insensitive: e.g., Aline or aline)", MetaName = "method")]
		public string Method { get; set; }

		[Option('r', "raw-scores", Default = false, HelpText = "Produce raw similarity scores (integers from 0 to infinity, where higher means more similar)")]
		public bool RawScores { get; set; }

		[Option('n', "normalized-scores", Default = false, HelpText = "Produce normalized similarity scores (real numbers between 0.0 and 1.0, where higher means more similar)")]
		public bool NormalizedScores { get; set; }

		[Option('v', "verbose", Default = false, HelpText = "Produce more verbose output, showing possible alignments (changes output format)")]
		public bool Verbose { get; set; }

		private Dictionary<string, Word> _parsedWords = new Dictionary<string, Word>();

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
				Errors.Add(e.Message);
				return null;
			}
			project.Segmenter.Segment(word);
			_parsedWords.Add(wordText, word);
			return word;
		}

		protected override ReturnCodes DoWork(TextReader inputReader, TextWriter outputWriter, TextWriter errorWriter)
		{
			ReturnCodes retcode = ReturnCodes.Okay;

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

			SetUpProject();

			WordAlignerBase wordAligner;
			switch (Method.ToLower())
			{
				case "aline":
					wordAligner = (Aline)Project.WordAligners["primary"];
					break;

				default:
					Warnings.Add("Unknown word aligner \"{0}\". Defaulting to Aline.", Method);
					wordAligner = (Aline)Project.WordAligners["primary"];
					break;
			}

			foreach (string line in ReadLines(inputReader))
			{
				string[] wordTexts = line.Split(' ');
				if (wordTexts.Length != 2)
				{
					Errors.Add(line, "Each line should have two space-separated words in it.");
					continue;
				}
				Word[] words = wordTexts.Select(wordText => ParseWordOnce(wordText, Meaning, Project)).ToArray();
				if (words.Length != 2 || words.Any(w => w == null))
				{
					Errors.Add(line, "One or more of this line's words failed to parse. Successfully parsed words: {0}", String.Join(", ", words.Where(w => w != null).Select(w => w.StrRep)));
					continue;
				}
				var result = wordAligner.Compute(words[0], words[1]);
				Alignment<Word, ShapeNode> alignment = result.GetAlignments().First();
				outputWriter.WriteLine("{0} {1} {2}", words[0].StrRep, words[1].StrRep, RawScores ? alignment.RawScore : alignment.NormalizedScore);
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
