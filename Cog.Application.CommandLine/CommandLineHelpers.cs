using System.Collections.Generic;
using System.IO;
using System.Reflection;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Config;
using SIL.Machine.Annotations;

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

		public static CogProject GetProject(SpanFactory<ShapeNode> spanFactory, SegmentPool segmentPool)
		{
			Stream stream = Assembly.GetAssembly(typeof(CommandLineHelpers)).GetManifestResourceStream("SIL.Cog.Application.CommandLine.NewProject.cogx");
			return ConfigManager.Load(spanFactory, segmentPool, stream);
		}
	}
}
