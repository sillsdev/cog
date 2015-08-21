using NUnit.Framework;

namespace SIL.Cog.Application.CommandLine.Tests
{
	[TestFixture]
	public class DistanceTests : TestBase
	{
		[Test, Sequential]
		public void CheckDistance(
			[Values(
				"chair apple",
				"apple chair",
				"chair cheer",
				"apple apple",
				"edit editable",
				"ˈɛdɪt ˈɛdɪtəbl̩")]
			string input,
			[Values(
				"|c h a - - - i r|\n|- - a p p l e -|\n650\n\n",
				"|- - a p p l e -|\n|c h a - - - i r|\n650\n\n",
				"|c h a i r|\n|c h e e r|\n16500\n\n",
				"|a p p l e|\n|a p p l e|\n17500\n\n",
				"|e d i t - - - -|\n|e d i t a b l e|\n10000\n\n",
				"|ɛ d ɪ t - - -|\n|ɛ d ɪ t ə b l̩|\n11000\n\n")]
			string expectedOutput)
		{
			var distancer = new DistanceVerb {Method = "Aline"};
			CheckVerbOutput(input, expectedOutput, distancer, false);
		}
	}
}
