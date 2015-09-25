using System;
using System.IO;
using CommandLine;
using SIL.Cog.Domain;

namespace SIL.Cog.CommandLine
{
	[Verb("syllabify", HelpText = "Syllabify one or many words")]
	public class SyllabifyVerb : VerbBase
	{
		protected override ReturnCodes DoWork(TextReader inputReader, TextWriter outputWriter, TextWriter errorWriter)
		{
			ReturnCodes retcode = ReturnCodes.Okay;
			SetUpProject();
			IProcessor<Variety> syllabifier = Project.VarietyProcessors["syllabifier"];

			foreach (string line in ReadLines(inputReader))
			{
				string wordText = line; // In the future we might need to split the line into multiple words
				Word word;
				try
				{
					word = ParseWord(wordText, Meaning);
					Project.Segmenter.Segment(word);
					Variety1.Words.Add(word);
				}
				catch (FormatException e)
				{
					Errors.Add(line, e.Message);
				}
			}
			syllabifier.Process(Variety1);
			foreach (Word word in Variety1.Words)
			{
//				output.WriteLine("{0} {1} {2}", word.StemIndex, word.StemLength, word.ToString().Replace(" ", ""));
				outputWriter.WriteLine(word.ToString().Replace(" ", ""));
			}
			return retcode;
		}
	}
}
