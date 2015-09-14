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

namespace SIL.Cog.CommandLine
{
	[Verb("syllabify", HelpText = "Syllabify one or many words")]
	public class SyllabifyVerb : CommonOptions
	{
		public override ReturnCodes DoWork(TextReader inputReader, TextWriter outputWriter, TextWriter errorWriter)
		{
			ReturnCodes retcode = ReturnCodes.Okay;
			SetUpProject();
			IProcessor<Variety> syllabifier = _project.VarietyProcessors["syllabifier"];

			foreach (string line in inputReader.ReadLines())
			{
				string wordText = line; // In the future we might need to split the line into multiple words
				Word word;
				try
				{
					word = ParseWord(wordText, _meaning);
					_project.Segmenter.Segment(word);
					_variety.Words.Add(word);
				}
				catch (FormatException e)
				{
					errors.Add(line, e.Message);
				}
			}
			syllabifier.Process(_variety);
			foreach (Word word in _variety.Words)
			{
//				output.WriteLine("{0} {1} {2}", word.StemIndex, word.StemLength, word.ToString().Replace(" ", ""));
				outputWriter.WriteLine(word.ToString().Replace(" ", ""));
			}
			return retcode;
		}
	}
}
