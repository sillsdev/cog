using System.IO;
using System.Reflection;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Config;

namespace SIL.Cog.TestUtils
{
	public static class TestHelpers
	{
		public static CogProject GetTestProject(SegmentPool segmentPool)
		{
			using (Stream stream = Assembly.GetAssembly(typeof(TestHelpers)).GetManifestResourceStream("SIL.Cog.TestUtils.TestProject.cogx"))
				return ConfigManager.Load(segmentPool, stream);
		}
	}
}
