﻿using System;
using System.IO;
using CommandLine;
using SIL.Cog.Domain;
using SIL.Machine.Annotations;

namespace SIL.Cog.Application.CommandLine
{
	[Verb("segment", HelpText = "Segment one or many words")]
	class SegmentVerb : CommonOptions
	{
		public override int DoWork(TextReader input, TextWriter output)
		{
			SpanFactory<ShapeNode> spanFactory = new ShapeSpanFactory();

			int retcode;
			var segmenter = new Segmenter(spanFactory)
			{
				Consonants = { "b", "c", "ch", "d", "f", "g", "h", "j", "k", "l", "m", "n", "p", "q", "r", "s", "sh", "t", "v", "w", "x", "z" },
				Vowels = { "a", "e", "i", "o", "u" },
				Boundaries = { "-" },
				Modifiers = { "\u0303", "\u0308" },
				Joiners = { "\u0361" }
			};

			retcode = (int) ReturnCodes.Okay;

			foreach(string line in input.ReadLines())
			{
				StreamWriter stderr = new StreamWriter(Console.OpenStandardError()); // For demo. Real implementation might allow logging erros to a file.

				string word = line; // For demo, one word per line. Real implementation might expect something like "word method" (space-separated) on each line.

				Shape shape;
				if (segmenter.TrySegment(word, out shape))
				{
					AnnotationList<ShapeNode> nodes = shape.Annotations;
					output.WriteLine(nodes.ToString());
				}
				else
				{
					stderr.WriteLine("Failed to parse {0}", word);
					retcode = (int)ReturnCodes.InputError;
				}
			}
			return retcode;
		}
	}

}