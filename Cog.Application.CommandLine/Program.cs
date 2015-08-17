using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using SIL.Cog.Domain;
using SIL.Machine.Annotations;

namespace SIL.Cog.Application.CommandLine
{
	class Program
	{
		static int Main(string[] args)
		{
			int retcode = Parser.Default.ParseArguments<SegmentOptions, CompareOptions>(args)
				.Return(
					(SegmentOptions opts) => DoSegmentation(opts),
					(CompareOptions opts) => { Console.WriteLine("Sorry, \"compare\" action not yet implemented."); return 2; },
					(errs) => 3);
			return retcode;
		}

		private static int DoSegmentation(SegmentOptions options)
		{
			SpanFactory<ShapeNode> spanFactory = new ShapeSpanFactory();

			Console.WriteLine("Input from " + options.InputFilename + " and output to " + options.OutputFilename);

			foreach (string word in options.Words)
			{
				Console.WriteLine("Will parse {0}", word);
				var segmenter = new Segmenter(spanFactory)
				{
					Consonants = { "b", "c", "ch", "d", "f", "g", "h", "j", "k", "l", "m", "n", "p", "q", "r", "s", "sh", "t", "v", "w", "x", "z" },
					Vowels = { "a", "e", "i", "o", "u" },
					Boundaries = { "-" },
					Modifiers = { "\u0303", "\u0308" },
					Joiners = { "\u0361" }
				};

				Shape shape;
				bool success = segmenter.TrySegment(word, out shape);
				if (success)
				{
					Console.WriteLine("Parsed {0}. Results:", word);
					var nodes = shape.Annotations;
					Console.WriteLine(nodes.ToString());
				}
				else
				{
					Console.WriteLine("Failed to parse {0}", word);
					return 1;
				}
			}
			return 0;
		}
	}
}
