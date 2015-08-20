using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace SIL.Cog.Application.CommandLine.Tests
{
	[TestFixture]
	public class DistanceTests
	{
		[Test, Sequential]
		public void CheckDistance(
			[Values("chair apple", "apple chair", "chair cheer", "apple apple")] string input,
			[Values("|chair| |apple| 650", "|apple| |chair| 650", "|chair| |cheer| 16500", "|apple| |apple| 17500")] string expectedOutput)
		{
			var inputStream = new StringReader(input);
			var outputStream = new StringWriter();
			var distancer = new DistanceVerb();
			distancer.Method = "Aline";
			var retcode = distancer.DoWork(inputStream, outputStream);
			string TextResult = outputStream.ToString().Replace("\r\n", "\n").Replace("\n", "");
			Assert.That(TextResult, Is.EqualTo(expectedOutput));
			Assert.That(retcode, Is.EqualTo(0));
		}
	}
}
