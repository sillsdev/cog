using NUnit.Framework;

namespace SIL.Cog.Application.CommandLine.Tests
{
	[TestFixture]
	public class DistanceTests : TestBase
	{
		[Test, Sequential]
		public void CheckDistanceWithRawScores(
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
				"|a|\n|a|\n3500\n\n",
				"|a -|\n|- b|\n-2000\n\n",
				"|c h a - - - i r|\n|- - a p p l e -|\n650\n\n",
				"|- - a p p l e -|\n|c h a - - - i r|\n650\n\n",
				"|c h a i r|\n|c h e e r|\n16500\n\n",
				"|a p p l e|\n|a p p l e|\n17500\n\n",
				"|e d i t - - - -|\n|e d i t a b l e|\n10000\n\n",
				"|ɛ d ɪ t - - -|\n|ɛ d ɪ t ə b l̩|\n11000\n\n")]
			string expectedOutput)
		{
			var distancer = new DistanceVerb { Method = "Aline", RawScores = true, NormalizedScores = false };
			CheckVerbOutput(input, expectedOutput, distancer, false);
		}

		[Test, Sequential]
		public void CheckDistanceWithNormalizedScores(
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
				"|a|\n|a|\n1\n\n",
				"|a -|\n|- b|\n0\n\n",
				"|c h a - - - i r|\n|- - a p p l e -|\n0.0371428571428571\n\n",
				"|- - a p p l e -|\n|c h a - - - i r|\n0.0371428571428571\n\n",
				"|c h a i r|\n|c h e e r|\n0.942857142857143\n\n",
				"|a p p l e|\n|a p p l e|\n1\n\n",
				"|e d i t - - - -|\n|e d i t a b l e|\n0.357142857142857\n\n",
				"|ɛ d ɪ t - - -|\n|ɛ d ɪ t ə b l̩|\n0.448979591836735\n\n")]
			string expectedOutput)
		{
			var distancer = new DistanceVerb { Method = "Aline", RawScores = false, NormalizedScores = true };
			CheckVerbOutput(input, expectedOutput, distancer, false);
		}

	}
}
