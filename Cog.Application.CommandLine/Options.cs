using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text; 


namespace SIL.Cog.Application.CommandLine
{
	abstract class CommonOptions
	{
		[Option('i', "input", Default = "-", HelpText = "Input filename (\"-\" for stdin)")]
		public string InputFilename { get; set; }

		[Option('o', "output", Default = "-", HelpText = "Output filename (\"-\" for stdout)")]
		public string OutputFilename { get; set; }
	}

	[Verb("segment", HelpText = "Segment one or many words")]
	class SegmentOptions : CommonOptions
	{
		[Value(0)]
		public IList<string> Words { get; set; }
	}

	[Verb("compare", HelpText = "Compare words (not yet implemented)")]
	class CompareOptions : CommonOptions
	{
		[Value(0)]
		public IList<string> Words { get; set; }
	}

}
