using NUnit.Framework;

namespace SIL.Cog.Application.CommandLine.Tests
{
	[TestFixture]
	public class DistanceTests : TestBase
	{
		[Test, Sequential]
		public void CheckDistance(
			[Values("chair apple", "apple chair", "chair cheer", "apple apple")] string input,
			[Values("|chair| |apple| 650", "|apple| |chair| 650", "|chair| |cheer| 16500", "|apple| |apple| 17500")] string expectedOutput)
		{
			var distancer = new DistanceVerb {Method = "Aline"};
			CheckVerbOutput(input, expectedOutput, distancer);
		}
	}
}
