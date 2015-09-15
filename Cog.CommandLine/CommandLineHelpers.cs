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
	}
}
