using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CommandLine;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;
using SIL.Cog.Domain.Config;
using SIL.Machine.Annotations;

namespace SIL.Cog.Application.CommandLine
{
	[Verb("syllabify", HelpText = "Syllabify one or many words")]
	public class SyllabifyVerb : CommonOptions
	{
		private readonly SpanFactory<ShapeNode> _spanFactory = new ShapeSpanFactory();

		public override int DoWork(TextReader input, TextWriter output)
		{
			var segmentPool = new SegmentPool();
			CogProject project = CommandLineHelpers.GetProject(_spanFactory, segmentPool);
			var variety = new Variety("variety1");
			var meaning = new Meaning("gloss1", "cat1");
			IProcessor<Variety> syllabifier = project.VarietyProcessors["syllabifier"];
			project.Meanings.Add(meaning);
			project.Varieties.Add(variety);
			foreach (string line in input.ReadLines())
			{
				string wordText = line; // In the future we might need to split the line into multiple words
				Word word = ParseWord(wordText, meaning);
				project.Segmenter.Segment(word);
				variety.Words.Add(word);
			}
			syllabifier.Process(variety);
			foreach (Word word in variety.Words)
			{
//				output.WriteLine("{0} {1} {2}", word.StemIndex, word.StemLength, word.ToString().Replace(" ", ""));
				output.WriteLine(word.ToString().Replace(" ", ""));
			}
			return 0;
		}
	}
}
