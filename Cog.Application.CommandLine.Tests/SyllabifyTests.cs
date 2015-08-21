using NUnit.Framework;

namespace SIL.Cog.Application.CommandLine.Tests
{
	[TestFixture]
	public class SyllabifyTests : TestBase
	{
		[Test, Sequential]
		public void CheckSyllabification(
			[Values("chair",   "apple",    "edit",    "ed.it",   "un.ed.it.a.ble",   "un.|ed.it|.a.ble")] string input,
			[Values("|chair|", "|ap.ple|", "|e.dit|", "|ed.it|", "|un.ed.it.a.ble|", "un.|ed.it|.a.ble")] string expectedOutput)
		{
			var syllabifier = new SyllabifyVerb();
			CheckVerbOutput(input, expectedOutput, syllabifier);
		}
	}
}
