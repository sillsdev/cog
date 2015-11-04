using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;
using SIL.Collections;

namespace SIL.Cog.CommandLine
{
	[Verb("cognates", HelpText = "Test words for cognicity")]
	public class CognatesVerb : VerbBase
	{
		protected override ReturnCodes DoWork(TextReader inputReader, TextWriter outputWriter, TextWriter errorWriter)
		{
			ReturnCodes retcode = ReturnCodes.Okay;

			SetUpProject();

			foreach (string line in ReadLines(inputReader))
			{
				Meaning meaning = MeaningFactory.Create();
				Project.Meanings.Add(meaning);
				string[] wordTexts = line.Split(' ');
				if (wordTexts.Length != 2)
				{
					Errors.Add(line, "Each line should have two space-separated words in it.");
					continue;
				}
				Word[] words = wordTexts.Select(wordText => ParseWord(wordText, meaning)).ToArray();
				if (words.Length != 2 || words.Any(w => w == null))
				{
					Errors.Add(line, "One or more of this line's words failed to parse. Successfully parsed words: {0}", String.Join(", ", words.Where(w => w != null).Select(w => w.StrRep)));
					continue;
				}

				Variety1.Words.Add(words[0]);
				Variety2.Words.Add(words[1]);
			}

			SegmentAll();

			foreach (VarietyPair varietyPair in Project.VarietyPairs)
			{
				Compare(varietyPair);
				foreach (WordPair wordPair in varietyPair.WordPairs)
				{
					// Output format: "word1 word2 True/False score" where True means cognate and False means not cognate, and score is a number between 0.0 and 1.0
					outputWriter.WriteLine("{0} {1} {2} {3}", wordPair.Word1.StrRep, wordPair.Word2.StrRep, wordPair.AreCognatePredicted, wordPair.CognacyScore);
				}
			}

			return retcode;
		}

		// Following code is from AnalysisService, tweaked just a little (e.g., not getting the project from ProjectService).
		// TODO: Refactor this, and/or AnalysisService, so that we don't have to have this code duplication.
		// (The code duplication is currently necessary because AnalysisService lives in Cog.Application, which references parts
		// of WPF like PresentationCore -- so we can't use Cog.Application with Mono on Linux. Moving AnalysisService to a
		// different assembly, or moving the WPF-dependent code to a different assembly, would be a good solution.) - 2015-09 RM
		public void Compare(VarietyPair varietyPair)
		{
			var pipeline = new Pipeline<VarietyPair>(GetCompareProcessors());
			pipeline.Process(varietyPair.ToEnumerable());
		}

		private IEnumerable<IProcessor<VarietyPair>> GetCompareProcessors()
		{
			var processors = new List<IProcessor<VarietyPair>>
				{
					Project.VarietyPairProcessors["wordPairGenerator"],
					new EMSoundChangeInducer(_segmentPool, Project, "primary", "primary"),
					new SoundCorrespondenceIdentifier(_segmentPool, Project, "primary")
				};
			return processors;
		}

		public void SegmentAll()
		{
			var pipeline = new MultiThreadedPipeline<Variety>(GetSegmentProcessors());
			pipeline.Process(Project.Varieties);
			pipeline.WaitForComplete();
		}

		public void Segment(Variety variety)
		{
			var pipeline = new Pipeline<Variety>(GetSegmentProcessors());
			pipeline.Process(variety.ToEnumerable());
		}

		private IEnumerable<IProcessor<Variety>> GetSegmentProcessors()
		{
			return new[]
				{
					new VarietySegmenter(Project.Segmenter),
					Project.VarietyProcessors["syllabifier"],
					new SegmentFrequencyDistributionCalculator(_segmentPool)
				};
		}
	}
}
