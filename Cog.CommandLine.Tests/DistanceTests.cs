using NUnit.Framework;

namespace SIL.Cog.CommandLine.Tests
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
				"a a 3500\n|a|\n|a|\n\n",
				"a b -2000\n|a -|\n|- b|\n\n",
				"chair apple 650\n|c h a - - - i r|\n|- - a p p l e -|\n\n",
				"apple chair 650\n|- - a p p l e -|\n|c h a - - - i r|\n\n",
				"chair cheer 16500\n|c h a i r|\n|c h e e r|\n\n",
				"apple apple 17500\n|a p p l e|\n|a p p l e|\n\n",
				"edit editable 10000\n|e d i t - - - -|\n|e d i t a b l e|\n\n",
				"ˈɛdɪt ˈɛdɪtəbl̩ 11000\n|ɛ d ɪ t - - -|\n|ɛ d ɪ t ə b l̩|\n\n")]
			string expectedOutput)
		{
			var distancer = new DistanceVerb { Method = "Aline", RawScores = true, NormalizedScores = false, Verbose = true };
			CheckVerbOutput(input, expectedOutput, distancer, false);
			distancer = new DistanceVerb { Method = "Aline", RawScores = true, NormalizedScores = false, Verbose = false };
			CheckVerbOutput(input, expectedOutput.Split('\n')[0], distancer, true);
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
				"a a 1\n|a|\n|a|\n\n",
				"a b 0\n|a -|\n|- b|\n\n",
				"chair apple 0.0371428571428571\n|c h a - - - i r|\n|- - a p p l e -|\n\n",
				"apple chair 0.0371428571428571\n|- - a p p l e -|\n|c h a - - - i r|\n\n",
				"chair cheer 0.942857142857143\n|c h a i r|\n|c h e e r|\n\n",
				"apple apple 1\n|a p p l e|\n|a p p l e|\n\n",
				"edit editable 0.357142857142857\n|e d i t - - - -|\n|e d i t a b l e|\n\n",
				"ˈɛdɪt ˈɛdɪtəbl̩ 0.448979591836735\n|ɛ d ɪ t - - -|\n|ɛ d ɪ t ə b l̩|\n\n")]
			string expectedOutput)
		{
			var distancer = new DistanceVerb { Method = "Aline", RawScores = false, NormalizedScores = true, Verbose = true };
			CheckVerbOutput(input, expectedOutput, distancer, false);
			distancer = new DistanceVerb { Method = "Aline", RawScores = false, NormalizedScores = true, Verbose = false };
			CheckVerbOutput(input, expectedOutput.Split('\n')[0], distancer, true);
		}

	}
}
