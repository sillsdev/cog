using System.IO;
using NUnit.Framework;

namespace SIL.Cog.CommandLine.Tests
{
	public class TestBase
	{
		public void CheckVerbOutput(string input, string expectedOutput, VerbBase verbUnderTest)
		{
			CheckVerbOutput(input, expectedOutput, verbUnderTest, true);
		}

		public void CheckVerbOutput(string input, string expectedOutput, VerbBase verbUnderTest, bool stripNewlines)
		{
			CheckVerbOutput(input, expectedOutput, null, verbUnderTest, stripNewlines);
		}

		public void CheckVerbOutput(string input, string expectedOutput, string expectedErrors, VerbBase verbUnderTest, bool stripNewlines)
		{
			var inputStream = new StringReader(input);
			var outputStream = new StringWriter();
			var errorStream = new StringWriter();
			var retcode = verbUnderTest.DoWorkWithErrorChecking(inputStream, outputStream, errorStream);
			string TextResult = outputStream.ToString().Replace("\r\n", "\n");
			string ErrorText = errorStream.ToString().Replace("\r\n", "\n");
			if (stripNewlines)
				TextResult = TextResult.Replace("\n", "");
			Assert.That(TextResult, Is.EqualTo(expectedOutput));
			if (expectedErrors == null)
			{
				Assert.That(ErrorText, Is.EqualTo(""));
				Assert.That(retcode, Is.EqualTo(ReturnCodes.Okay));
			}
			else
			{
				Assert.That(ErrorText, Is.EqualTo(expectedErrors));
				Assert.That(retcode, Is.Not.EqualTo(ReturnCodes.Okay));
			}
		}

	}
}
