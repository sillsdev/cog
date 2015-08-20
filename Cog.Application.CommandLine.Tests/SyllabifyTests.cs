using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace SIL.Cog.Application.CommandLine.Tests
{
	[TestFixture]
	public class SyllabifyTests
	{
		[Test, Sequential]
		public void CheckSyllabification(
			[Values("chair",   "apple",    "edit",    "ed.it",   "un.ed.it.a.ble",   "un.|ed.it|.a.ble")] string input,
			[Values("|chair|", "|ap.ple|", "|e.dit|", "|ed.it|", "|un.ed.it.a.ble|", "un.|ed.it|.a.ble")] string expectedOutput)
		{
			var inputStream = new StringReader(input);
			var outputStream = new StringWriter();
			var syllabifier = new SyllabifyVerb();
			var retcode = syllabifier.DoWork(inputStream, outputStream);
			string TextResult = outputStream.ToString().Replace("\r\n", "\n").Replace("\n", "");
			Assert.That(retcode, Is.EqualTo(0));
			Assert.That(TextResult, Is.EqualTo(expectedOutput));
		}
	}
}
