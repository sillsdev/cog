using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text; 


namespace SIL.Cog.Application.CommandLine
{
	class CogCommandLineOptions
	{
		[Option('i', "input", DefaultValue = "-", HelpText = "Input filename (\"-\" for stdin)")]
		public string InputFilename { get; set; }

		[Option('o', "output", DefaultValue = "-", HelpText = "Output filename (\"-\" for stdout)")]
		public string OutputFilename { get; set; }

		[ValueList(typeof(List<string>))]
		public IList<string> OtherArgs { get; set; }

		[HelpOption]
		public string GetUsage()
		{
			return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
		}
	}
}
