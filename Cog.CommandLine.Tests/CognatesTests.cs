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
				"a a True",
				"a b False",
				"chair apple False",
				"apple chair False",
				"chair cheer True",
				"apple apple True",
				"edit editable False",
				"ˈɛdɪt ˈɛdɪtəbl̩ False")]
			string expectedOutput)
		{
			var aligner = new CognatesVerb();
			CheckVerbOutput(input, expectedOutput, aligner, true);
		}
	}
}
