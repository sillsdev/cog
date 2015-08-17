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
			var options = new CogCommandLineOptions();
			var parsed = Parser.Default.ParseArguments(args, options);

			SpanFactory<ShapeNode> spanFactory = new ShapeSpanFactory();
			if (parsed)
			{
				Console.WriteLine("Input from " + options.InputFilename + " and output to " + options.OutputFilename);
				Console.WriteLine("Other args: " + String.Join(", ", options.OtherArgs));

				foreach (string word in options.OtherArgs)
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
			else
			{
				// Console.WriteLine("Failed to parse options...");
				// No need to print anything as the HelpOption function has already printed appropriate usage info
			}
		}
	}
}
