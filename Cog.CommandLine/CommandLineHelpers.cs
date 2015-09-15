using System;
using System.Collections.Generic;
using System.IO;

namespace SIL.Cog.CommandLine
{
	public static class CommandLineHelpers
	{
		public static IEnumerable<string> ReadLines(this TextReader input)
		{
			string line;
			while ((line = input.ReadLine()) != null)
			{
				if (!string.IsNullOrWhiteSpace(line)) // Silently skip blank lines
					yield return line;
			}
		}

		public static string CountedNoun(int count, string singular)
		{
			return CountedNoun(count, singular, singular + "s");
		}

		public static string CountedNoun(int count, string singular, string plural)
		{
			return string.Format("{0} {1}", count, count == 1 ? singular : plural);
		}
	}
}
