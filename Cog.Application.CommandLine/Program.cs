using CommandLine;

namespace SIL.Cog.Application.CommandLine
{
	enum ReturnCodes
	{
		Okay = 0,
		InputError = 1,
		NotImplemented = 2,
		UnknownVerb = 3
	}

	class Program
	{
		static int Main(string[] args)
		{
			int retcode = Parser.Default.ParseArguments<SegmentVerb, CompareVerb>(args)
				.Return(
					(SegmentVerb opts) => opts.RunAsPipe(),
					(CompareVerb opts) => opts.RunAsPipe(),
					(errs) => (int)ReturnCodes.UnknownVerb);
			return retcode;
		}
	}

}
