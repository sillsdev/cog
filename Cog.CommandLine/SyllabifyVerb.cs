using System;
using System.IO;
using CommandLine;
using SIL.Cog.Domain;

namespace SIL.Cog.CommandLine
{
	[Verb("syllabify", HelpText = "Syllabify one or many words")]
	public class SyllabifyVerb : VerbBase
	{
		protected override ReturnCode DoWork(TextReader inputReader, TextWriter outputWriter, TextWriter errorWriter)
		{
			ReturnCode retcode = ReturnCode.Okay;
			SetupProject();
			var variety = new Variety("variety1");
			Project.Varieties.Add(variety);
			Meaning meaning = MeaningFactory.Create();
			Project.Meanings.Add(meaning);
			IProcessor<Variety> syllabifier = Project.VarietyProcessors[ComponentIdentifiers.Syllabifier];

			foreach (string line in ReadLines(inputReader))
			{
				string wordText = line; // In the future we might need to split the line into multiple words
				try
				{
					Word word = ParseWord(wordText, meaning);
					Project.Segmenter.Segment(word);
					variety.Words.Add(word);
				}
				catch (FormatException e)
				{
					Errors.Add(line, e.Message);
				}
			}
			syllabifier.Process(variety);
			foreach (Word word in variety.Words)
			{
//				output.WriteLine("{0} {1} {2}", word.StemIndex, word.StemLength, word.ToString().Replace(" ", ""));
				outputWriter.WriteLine(word.ToString().Replace(" ", ""));
			}
			return retcode;
		}
	}
}
