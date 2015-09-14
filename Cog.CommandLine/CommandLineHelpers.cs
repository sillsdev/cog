using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Config;
using SIL.Machine.Annotations;

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

		public static CogProject GetProjectFromResource(SpanFactory<ShapeNode> spanFactory, SegmentPool segmentPool)
		{
			Stream stream = Assembly.GetAssembly(typeof(CommandLineHelpers)).GetManifestResourceStream("SIL.Cog.CommandLine.NewProject.cogx");
			return ConfigManager.Load(spanFactory, segmentPool, stream);
		}

		public static CogProject GetProjectFromFilename(SpanFactory<ShapeNode> spanFactory, SegmentPool segmentPool, string projectFilename)
		{
			if (projectFilename == null)
			{
				return GetProjectFromResource(spanFactory, segmentPool);
			}
			else
			{
				return ConfigManager.Load(spanFactory, segmentPool, projectFilename);
			}
		}

		public static CogProject GetProjectFromXmlString(SpanFactory<ShapeNode> spanFactory, SegmentPool segmentPool, string xmlString)
		{
			return ConfigManager.LoadFromXmlString(spanFactory, segmentPool, xmlString);
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
