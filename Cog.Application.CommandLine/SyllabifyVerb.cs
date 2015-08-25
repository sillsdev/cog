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
		public override ReturnCodes DoWork(TextReader inputStream, TextWriter outputStream, TextWriter errorStream)
		{
			ReturnCodes retcode = ReturnCodes.Okay;
			SetUpProject();
			IProcessor<Variety> syllabifier = _project.VarietyProcessors["syllabifier"];

			foreach (string line in inputStream.ReadLines())
			{
				string wordText = line; // In the future we might need to split the line into multiple words
				Word word = ParseWord(wordText, _meaning);
				_project.Segmenter.Segment(word);
				_variety.Words.Add(word);
			}
			syllabifier.Process(_variety);
			foreach (Word word in _variety.Words)
			{
//				output.WriteLine("{0} {1} {2}", word.StemIndex, word.StemLength, word.ToString().Replace(" ", ""));
				outputStream.WriteLine(word.ToString().Replace(" ", ""));
			}
			return retcode;
		}
	}
}
