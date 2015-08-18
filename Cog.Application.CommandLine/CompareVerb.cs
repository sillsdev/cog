using System;
using System.Collections.Generic;
using System.IO;
using CommandLine;

namespace SIL.Cog.Application.CommandLine
{
	[Verb("compare", HelpText = "Compare words (not yet implemented)")]
	class CompareVerb : CommonOptions
	{
		[Value(0)]
		public IList<string> Words { get; set; } // Not actually used for much, but this shows how to get a word list on the command line

		public override int DoWork(StreamReader input, StreamWriter output)
		{
			Console.WriteLine("Sorry, \"compare\" action not yet implemented.");
			Console.WriteLine("But I did get the list of words you wanted to compare: {0}", String.Join(", ", Words));
			return (int)ReturnCodes.NotImplemented;
		}
	}
}
