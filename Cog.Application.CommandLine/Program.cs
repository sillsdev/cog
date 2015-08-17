using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using SIL.Cog.Domain;
using SIL.Machine.Annotations;

namespace SIL.Cog.Application.CommandLine
{
	class Program
	{
		static void Main(string[] args)
		{
			ParserResult<object> parsed = Parser.Default.ParseArguments<SegmentOptions, CompareOptions>(args)
				.WithParsed<SegmentOptions>(options =>
				{
					Console.WriteLine("Going to parse {0}", String.Join(", ", options.Words));
					DoSegmentation(options);
				})
				.WithParsed<CompareOptions>(options =>
				{
					Console.WriteLine("ERROR: Compare not yet implemented, sorry.");
				})
				.WithNotParsed(errors =>
				{
					// Displaying errors is taken care of by the CommandLineParser library, so we don't need the following.
					//foreach (var error in errors)
					//{
					//	Console.WriteLine("Unknown option -{0}", ((UnknownOptionError)error).Token);
					//}
					Console.WriteLine("Please fix the error{0} listed above, then try again.", (errors.Count() > 1) ? "s" : "");
				});
		}

		private static void DoSegmentation(SegmentOptions options)
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
				}
			}
		}
	}
}
