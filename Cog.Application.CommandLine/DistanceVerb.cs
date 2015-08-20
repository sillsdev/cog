using System;
using System.Collections.Generic;
using System.IO;
using CommandLine;

namespace SIL.Cog.Application.CommandLine
{
	[Verb("distance", HelpText = "Distance between words (not yet implemented)")]
	class DistanceVerb : CommonOptions
	{
		[Value(0)]
		public IList<string> Words { get; set; } // Not actually used for much, but this shows how to get a word list on the command line

		public override int DoWork(TextReader input, TextWriter output)
		{
			Console.WriteLine("Sorry, \"distance\" action not yet implemented.");
			Console.WriteLine("But I did get the list of words you wanted to compare by distance: {0}", String.Join(", ", Words));
			return (int)ReturnCodes.NotImplemented;
		}
	}
}
