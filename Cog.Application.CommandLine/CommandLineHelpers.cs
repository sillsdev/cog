using System.Collections.Generic;
using System.IO;

namespace SIL.Cog.Application.CommandLine
{
	public static class CommandLineHelpers
	{
		public static IEnumerable<string> ReadLines(this TextReader input)
		{
			string line;
			while ((line = input.ReadLine()) != null)
			{
				yield return line;
			}
		}
	}
}