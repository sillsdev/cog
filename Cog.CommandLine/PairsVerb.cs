using System;
using System.Collections.Generic;
using System.IO;
using CommandLine;

namespace SIL.Cog.CommandLine
{
	[Verb("pairs", HelpText = "Turn a list of words into unique word pairs")]
	public class PairsVerb : VerbBase
	{
		protected override ReturnCode DoWork(TextReader inputReader, TextWriter outputWriter, TextWriter errorWriter)
		{
			ReturnCode retcode = ReturnCode.Okay;
			var words = new List<string>();
			foreach (string line in ReadLines(inputReader))
			{
				words.Add(line);
			}

			foreach (Tuple<string, string> wordPair in AllPossiblePairs(words))
			{
				outputWriter.WriteLine("{0} {1}", wordPair.Item1, wordPair.Item2);
			}

			return retcode;
		}
	}
}
