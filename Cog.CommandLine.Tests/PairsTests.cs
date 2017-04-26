using NUnit.Framework;

namespace SIL.Cog.CommandLine.Tests
{
	[TestFixture]
	public class PairsTests : TestBase
	{
		[Test, Sequential]
		public void MakePairsProducesAllCombinations(
			[Values("one", "one\r\ntwo", "one\ntwo\n", "1\r\n2\r\n3\r\n", "1\n2\n3\n4")]
			string input,
			[Values("",    "one two\n",  "one two\n",  "1 2\n1 3\n2 3\n", "1 2\n1 3\n1 4\n2 3\n2 4\n3 4\n")]
			string expectedOutput)
		{
			var pairs = new PairsVerb();
			CheckVerbOutput(input, expectedOutput, pairs, stripNewlines:false);
		}
	}
}
