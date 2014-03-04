using System.IO;
using System.Reflection;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Config;
using SIL.Machine.Annotations;

namespace SIL.Cog.Application.Tests
{
	public static class TestHelpers
	{
		public static CogProject GetTestProject(SpanFactory<ShapeNode> spanFactory, SegmentPool segmentPool)
		{
			Stream stream = Assembly.GetAssembly(typeof(TestHelpers)).GetManifestResourceStream("SIL.Cog.Application.Tests.TestProject.cogx");
			return ConfigManager.Load(spanFactory, segmentPool, stream);
		}
	}
}
