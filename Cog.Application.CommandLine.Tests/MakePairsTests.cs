using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SIL.Cog.Application.CommandLine.Tests
{
	[TestFixture]
    public class MakePairsTests
	{
		private TextReader InputStream;
		private TextWriter OutputStream;

		[Test, Sequential]
		public void MakePairsProducesAllCombinations(
			[Values("one", "one\r\ntwo", "one\ntwo\n", "1\r\n2\r\n3\r\n", "1\n2\n3\n4")]
			string textInput,
			[Values("",    "one two\n",  "one two\n",  "1 2\n1 3\n2 3\n", "1 2\n1 3\n1 4\n2 3\n2 4\n3 4\n")]
			string expectedOutput)
		{
			InputStream = new StringReader(textInput);
			OutputStream = new StringWriter();
			var makePairs = new MakePairsVerb();
			var retcode = makePairs.DoWork(InputStream, OutputStream);
			string TextResult = OutputStream.ToString().Replace("\r\n", "\n");
			Assert.That(retcode, Is.EqualTo(0));
			Assert.That(TextResult, Is.EqualTo(expectedOutput));
		}
	}
}
