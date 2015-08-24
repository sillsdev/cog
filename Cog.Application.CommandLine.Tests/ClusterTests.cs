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
			[Values(
				"a b 0.5\na c 0.4\na d 0.999\nb c 0.3\nb d 0.5\nc d 0.4\n",
				"a b 0.5\na c 0.4\na d 0.801\nb c 0.3\nb d 0.5\nc d 0.4\n", // Pair of words, a and d, JUST above the threshhold
				"a b 0.5\na c 0.4\na d 0.799\nb c 0.3\nb d 0.5\nc d 0.4\n"  // Pair of words, a and d, JUST below the threshhold
				)]
			string input,
			[Values(
				"1 b\n2 c\n3 a d\n",
				"1 b\n2 c\n3 a d\n",   // Just above the threshhold: a and d are grouped together
				"1 a\n2 b\n3 c\n4 d\n" // Just below the threshhold: a and d are not grouped
				)]
			string expectedOutput)
		{
			var clusterer = new ClusterVerb() { Threshhold = 0.2 };
			CheckVerbOutput(input, expectedOutput, clusterer, false);
		}
	}
}
