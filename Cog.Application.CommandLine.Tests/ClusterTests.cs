using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace SIL.Cog.Application.CommandLine.Tests
{
	class ClusterTests : TestBase
	{
		[Test, Sequential]
		public void CheckDistance(
			[Values("a b 0.5\na c 0.4\na d 0.999\nb c 0.3\nb d 0.5\nc d 0.4\n")] string input,
			[Values("1 b\n2 c\n3 a d\n")] string expectedOutput)
		{
			var clusterer = new ClusterVerb() { Threshhold = 0.2 };
			CheckVerbOutput(input, expectedOutput, clusterer, false);
		}
	}
}
