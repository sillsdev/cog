using NUnit.Framework;

namespace SIL.Cog.CommandLine.Tests
{
	[TestFixture]
	public class CognatesTests : TestBase
	{
		[Test, Sequential]
		public void CheckCognates(
			[Values(
				"a a",
				"a b",
				"chair apple",
				"apple chair",
				"chair cheer",
				"apple apple",
				"edit editable",
				"ˈɛdɪt ˈɛdɪtəbl̩")]
			string input,
			[Values(
				"a a True 1",
				"a b False 0",
				"chair apple False 0.2",
				"apple chair False 0.2",
				"chair cheer True 0.75",
				"apple apple True 1",
				"edit editable False 0.571428571428571",
				"ˈɛdɪt ˈɛdɪtəbl̩ False 0.666666666666667")]
			string expectedOutput)
		{
			var aligner = new CognatesVerb();
			CheckVerbOutput(input, expectedOutput, aligner, true);
		}
	}
}
