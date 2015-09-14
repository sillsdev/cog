using System;
using System.Collections.Generic;
using System.IO;
using CommandLine;
using System.Linq;
using System.Text;

namespace SIL.Cog.CommandLine
{
	[Verb("make-pairs", HelpText = "Turn a list of words into unique word pairs")]
	public class MakePairsVerb : VerbBase
	{
		public override ReturnCodes DoWork(TextReader inputReader, TextWriter outputWriter, TextWriter errorWriter)
		{
			ReturnCodes retcode = ReturnCodes.Okay;
			var words = new List<string>();
			foreach (string line in inputReader.ReadLines())
			{
				words.Add(line);
			}

			foreach (var wordPair in AllPossiblePairs(words))
			{
				outputWriter.WriteLine("{0} {1}", wordPair.Item1, wordPair.Item2);
			}

			return retcode;
		}

		protected IEnumerable<Tuple<string, string>> AllPossiblePairs(IEnumerable<string> words)
		{
			var queue = new Queue<string>(words);
			while (queue.Count > 0) // This is O(1), because Queue<T> keeps track of its count
			{
				string first = queue.Dequeue();
				foreach (string second in queue)
					yield return new Tuple<string, string>(first, second);
			}
		} 
	}
}
