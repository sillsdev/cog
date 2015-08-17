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
		[Option('i', "input", DefaultValue = "-", HelpText = "Input filename (\"-\" for stdin)")]
		public string InputFilename { get; set; }

		[Option('o', "output", DefaultValue = "-", HelpText = "Output filename (\"-\" for stdout)")]
		public string OutputFilename { get; set; }

		[HelpOption]
		public string GetUsage()
		{
			return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current), true);
		}
	}

	class SegmentOptions : CommonOptions
	{
		[ValueList(typeof(List<string>))]
		public IList<string> Words { get; set; }
	}

	class CompareOptions : CommonOptions
	{
		[ValueList(typeof(List<string>))]
		public IList<string> Words { get; set; }
	}

	class Options
	{
		[VerbOption("segment", HelpText = "Segment one or many words")]
		public SegmentOptions SegmentVerb { get; set; }

		[VerbOption("compare", HelpText = "Compare words (not yet implemented)")]
		public CompareOptions CompareVerb { get; set; }

		[HelpVerbOption]
		public string GetUsage(string verb)
		{
			Console.WriteLine("Need help for {0}", verb);
			return HelpText.AutoBuild(this, verb);
		}
	}
}
