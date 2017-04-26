using System;
using CommandLine;

namespace SIL.Cog.CommandLine
{
	class Program
	{
		private static readonly Type[] ValidVerbs = {
			typeof(SyllabifyVerb),
			typeof(PairsVerb),
			typeof(AlignmentVerb),
			typeof(CognatesVerb),
			typeof(ClusterVerb),
			typeof(SimilarityVerb)
		};
		static int Main(string[] args)
		{
			ParserResult<object> parserResult = Parser.Default.ParseArguments(args, ValidVerbs);
			ReturnCode retcode = parserResult.MapResult((VerbBase opts) => opts.RunAsPipe(), errs => ReturnCode.InputError);
			return (int)retcode;
		}
	}
}
