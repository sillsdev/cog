using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CommandLine;
using SIL.Cog.Domain;
using SIL.Machine.Annotations;

namespace SIL.Cog.Application.CommandLine
{
	public abstract class CommonOptions
	{
		[Option('i', "input", Default = "-", HelpText = "Input filename (\"-\" for stdin)")]
		public string InputFilename { get; set; }

		[Option('o', "output", Default = "-", HelpText = "Output filename (\"-\" for stdout)")]
		public string OutputFilename { get; set; }

		protected SpanFactory<ShapeNode> _spanFactory;
		protected SegmentPool _segmentPool;
		protected CogProject _project;
		protected Variety _variety;
		protected Meaning _meaning;

		protected Errors errors = new Errors();
		protected Errors warnings = new Errors();

		protected void SetUpProject()
		{
			_spanFactory = new ShapeSpanFactory();
			_segmentPool = new SegmentPool();
			_variety = new Variety("variety1");
			_meaning = new Meaning("gloss1", "cat1");
			_project = CommandLineHelpers.GetProject(_spanFactory, _segmentPool);
			_project.Meanings.Add(_meaning);
			_project.Varieties.Add(_variety);
		}

		public ReturnCodes DoWorkWithErrorChecking(TextReader inputReader, TextWriter outputWriter, TextWriter errorWriter)
		{
			ReturnCodes retcode = DoWork(inputReader, outputWriter, errorWriter);
			if (!warnings.Empty)
			{
				warnings.Write(errorWriter);
				// Do not change retcode for warnings
			}
			if (!errors.Empty)
			{
				errors.Write(errorWriter);
				retcode = (retcode == ReturnCodes.Okay) ? ReturnCodes.InputError : retcode;
			}
			return retcode;
		}

		public abstract ReturnCodes DoWork(TextReader inputReader, TextWriter outputWriter, TextWriter errorWriter);

		public ReturnCodes RunAsPipe()
		{
			return RunAsPipe(Console.Error);
		}

		public ReturnCodes RunAsPipe(TextWriter errorWriter)
		{
			using (StreamReader input = OpenInput())
			using (StreamWriter output = OpenOutput())
				return DoWorkWithErrorChecking(input, output, errorWriter);
		}

		protected Word ParseWord(string wordText, Meaning meaning)
		{
			int stemStartIdx = wordText.IndexOf("|", StringComparison.Ordinal);
			int stemEndIdx = wordText.LastIndexOf("|", StringComparison.Ordinal) - 1; // -1 because we're going to remove the leading |
			if (stemStartIdx != -1 && stemEndIdx < stemStartIdx)
			{
				// Only way this can happen is if there was only a single "|" in the word
				throw new FormatException(string.Format("Words should have either 0 or 2 pipe characters representing word stems. Offending word: ", wordText));
			}
			var word = (stemStartIdx == -1) ?
				new Word(wordText, meaning) :
				new Word(wordText.Replace("|", ""), stemStartIdx, stemEndIdx - stemStartIdx, meaning);
			return word;
		}

		#region OpenInput and overloads

		protected StreamReader OpenInput()
		{
			return OpenInput(InputFilename);
		}

		protected StreamReader OpenInput(string filename)
		{
			return OpenInput(filename, new UTF8Encoding());
		}

		protected StreamReader OpenInput(Encoding encoding)
		{
			return OpenInput(InputFilename, encoding);
		}

		protected StreamReader OpenInput(string filename, Encoding encoding)
		{
			Stream source = filename == "-"
				? Console.OpenStandardInput()
				: new FileStream(filename, FileMode.Open, FileAccess.Read);
			return new StreamReader(source, encoding);
		}
		#endregion
		#region OpenOutput and overloads
		protected StreamWriter OpenOutput()
		{
			return OpenOutput(OutputFilename);
		}

		protected StreamWriter OpenOutput(string filename)
		{
			return OpenOutput(filename, new UTF8Encoding());
		}

		protected StreamWriter OpenOutput(Encoding encoding)
		{
			return OpenOutput(OutputFilename, encoding);
		}

		protected StreamWriter OpenOutput(string filename, Encoding encoding)
		{
			Stream source = filename == "-"
				? Console.OpenStandardOutput()
				: new FileStream(filename, FileMode.Create, FileAccess.Write);
			return new StreamWriter(source, encoding);
		}

		#endregion
	}
}
