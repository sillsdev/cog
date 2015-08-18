using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using SIL.Cog.Domain;
using SIL.Machine.Annotations;

enum ReturnCodes
{
	Okay = 0,
	InputError,
	NotImplemented,
	UnknownVerb
}

namespace SIL.Cog.Application.CommandLine
{
	class Program
	{
		static int Main(string[] args)
		{
			int retcode = Parser.Default.ParseArguments<SegmentOptions, CompareOptions>(args)
				.Return(
					(SegmentOptions opts) => DoSegmentation(opts),
					(CompareOptions opts) => DoCompare(opts),
					(errs) => (int)ReturnCodes.UnknownVerb);
			return retcode;
		}

		private static StreamReader OpenInput(string filename)
		{
			return OpenInput(filename, new UTF8Encoding());
		}

		private static StreamReader OpenInput(string filename, Encoding encoding)
		{
			Stream source = filename == "-" ? Console.OpenStandardInput() : new FileStream(filename, FileMode.Open, FileAccess.Read);
			return new StreamReader(source, encoding);
		}

		private static StreamWriter OpenOutput(string filename)
		{
			return OpenOutput(filename, new UTF8Encoding());
		}

		private static StreamWriter OpenOutput(string filename, Encoding encoding)
		{
			Stream source = filename == "-" ? Console.OpenStandardOutput() : new FileStream(filename, FileMode.Create, FileAccess.Write);
			return new StreamWriter(source, encoding);
		}

		private static int DoSegmentation(SegmentOptions options)
		{
			SpanFactory<ShapeNode> spanFactory = new ShapeSpanFactory();

			StreamReader Input = OpenInput(options.InputFilename);
			StreamWriter Output = OpenOutput(options.OutputFilename);

			var segmenter = new Segmenter(spanFactory)
			{
				Consonants = { "b", "c", "ch", "d", "f", "g", "h", "j", "k", "l", "m", "n", "p", "q", "r", "s", "sh", "t", "v", "w", "x", "z" },
				Vowels = { "a", "e", "i", "o", "u" },
				Boundaries = { "-" },
				Modifiers = { "\u0303", "\u0308" },
				Joiners = { "\u0361" }
			};

			int retcode = (int)ReturnCodes.Okay;

			while (!Input.EndOfStream)
			{
				string line = Input.ReadLine();
				if (line == null)
					break;

				StreamWriter stderr = new StreamWriter(Console.OpenStandardError()); // For demo. Real implementation might allow logging to a file.

				string word = line; // For demo, one word per line. Real implementation might expect something like "word method" (space-separated) on each line.

				Shape shape;
				if (segmenter.TrySegment(word, out shape))
				{
					AnnotationList<ShapeNode> nodes = shape.Annotations;
					Output.WriteLine(nodes.ToString());
				}
				else
				{
					stderr.WriteLine("Failed to parse {0}", word);
					retcode = (int)ReturnCodes.InputError;
				}
			}
			Input.Close();
			Output.Close();
			return retcode;
		}

		private static int DoCompare(CompareOptions options)
		{
			Console.WriteLine("Sorry, \"compare\" action not yet implemented.");
			return (int)ReturnCodes.NotImplemented;
		}
	}
}
