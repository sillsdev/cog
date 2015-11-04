using System;
using CommandLine;

namespace SIL.Cog.CommandLine
{
	class Program
	{
		private static readonly Type[] ValidVerbs = {
			typeof (SyllabifyVerb),
			typeof (PairsVerb),
			typeof (AlignmentVerb),
			typeof (CognatesVerb),
			typeof (ClusterVerb)
		};
		static int Main(string[] args)
		{
			ParserResult<object> parserResult = Parser.Default.ParseArguments(args, ValidVerbs);
			ReturnCodes retcode = parserResult.MapResult((VerbBase opts) => opts.RunAsPipe(), errs => ReturnCodes.InputError);
			return (int)retcode;
		}
	}
}
