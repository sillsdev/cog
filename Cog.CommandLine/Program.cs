using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using CommandLine.Text;

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
			var parser = new Parser(config => config.HelpWriter = null);
			ParserResult<object> parserResult = parser.ParseArguments(args, ValidVerbs);
			if (parserResult.Tag == ParserResultType.NotParsed)
			{
				return (int)HandleParseErrors(parserResult);
			}
			ReturnCodes retcode = parserResult.MapResult((VerbBase opts) => opts.RunAsPipe(), errs => ReturnCodes.InputError);
			return (int)retcode;
		}

		static ReturnCodes HandleParseErrors(ParserResult<object> parserResult)
		{
			IEnumerable<Error> errs = ((NotParsed<object>) parserResult).Errors;
			IList<Error> errorList = errs as IList<Error> ?? errs.ToList();
			var helpText = new HelpText()
			{
				Heading = HeadingInfo.Default,
				Copyright = CopyrightInfo.Default,
				AdditionalNewLineAfterOption = false,
				AddDashesToOption = true
			};
			string verb = "";
			Type verbType;
			foreach (Error e in errorList)
			{
				if (e.Tag == ErrorType.HelpVerbRequestedError)
				{
					verb = ((HelpVerbRequestedError)e).Verb;
					verbType = ((HelpVerbRequestedError)e).Type;
				}
			}
			helpText = HelpText.DefaultParsingErrorsHandler<object>(parserResult, helpText);
			if (errorList.Any(e => e is NoVerbSelectedError))
			{
				helpText.AddDashesToOption = false;
				helpText.AddVerbs(ValidVerbs);
			}
			else if (errorList.Any(e => e is HelpVerbRequestedError))
			{
				helpText.AddPreOptionsLine("Valid options for \"" + verb + "\" operation:");
				helpText.AddOptions(parserResult);
			}
			else
			{
				helpText.AddOptions(parserResult);
			}
			
			Console.Error.WriteLine(helpText);
			if (errorList.Any(err => (err is HelpRequestedError || err is HelpVerbRequestedError)))
			{
				// Help requested; not really an error
				return ReturnCodes.Okay;
			}
			return ReturnCodes.UnknownVerb;
		}
	}
}
