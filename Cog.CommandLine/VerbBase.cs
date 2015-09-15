using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using CommandLine;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Config;
using SIL.Machine.Annotations;

namespace SIL.Cog.CommandLine
{
	public class VerbBase // Non-abstract so we can unit test it
	{
		[Option('i', "input", Default = "-", HelpText = "Input filename (\"-\" for stdin)")]
		public string InputFilename { get; set; }

		[Option('o', "output", Default = "-", HelpText = "Output filename (\"-\" for stdout)")]
		public string OutputFilename { get; set; }

		[Option('c', "config-file", HelpText = "Config file to use instead of default config (--config-data will override this, if passed)")]
		public string ConfigFilename { get; set; }

		[Option("config-data", HelpText = "Configuration to use, as a single long string (if passed, overrides --config-file)")]
		public string ConfigData { get; set; }

		protected SpanFactory<ShapeNode> _spanFactory;
		protected SegmentPool _segmentPool;
		public CogProject _project; // Public because unit tests need access to it. TODO: Turn this (and the other protected members) into proper properties.
		protected Variety _variety;
		protected Meaning _meaning;

		protected Errors errors = new Errors();
		protected Errors warnings = new Errors();

		public virtual string GetVerbName()
		{
			foreach (VerbAttribute attrib in GetType().GetCustomAttributes(typeof(VerbAttribute), true))
			{
				return attrib.Name;
			}
			return String.Empty;
		}

		protected IEnumerable<string> PossibleConfigFilenames
		{ 
			get
			{
				// We're slightly violating the XDG spec here: we look for the user's config in $XDG_CONFIG_HOME, but if
				// he doesn't have a config file there, we get the default from $XDG_DATA_DIRS rather than $XDG_CONFIG_DIRS.
				string assemblyName = typeof(Program).Assembly.GetName().Name;
				string configFileName = assemblyName + ".conf";
				yield return Path.Combine(XdgDirectories.ConfigHome, configFileName);
				foreach (string BaseDir in XdgDirectories.DataDirs)
					yield return Path.Combine(BaseDir, configFileName);
			}
		}

		protected string FindConfigFilename()
		{
			// Note that we can't validate the config files due to the Mono XSD validation bug, so we just look for the first one that exists.
			foreach (string candidateFilename in PossibleConfigFilenames)
				if (File.Exists(candidateFilename))
					return candidateFilename;
			return null;
		}

		public void SetUpProject() // Should this be protected? But the unit test needs to call it.
		{
			if (ConfigData != null && ConfigFilename != null)
			{
				warnings.Add("WARNING: options --config-data and --config-file were both specified. Ignoring --config-file.");
				ConfigFilename = null;
			}
			if (ConfigData == null && ConfigFilename == null)
			{
				ConfigFilename = FindConfigFilename();
				// If ConfigFilename is STILL null at this point, it's because no config files were found at all, so we'll use the default one from the resource.
			}
			_spanFactory = new ShapeSpanFactory();
			_segmentPool = new SegmentPool();
			_variety = new Variety("variety1");
			_meaning = new Meaning("gloss1", "cat1");
			if (ConfigData == null && ConfigFilename == null)
				_project = GetProjectFromResource(_spanFactory, _segmentPool);
			else if (ConfigData != null)
				_project = GetProjectFromXmlString(_spanFactory, _segmentPool, ConfigData);
			else if (ConfigFilename != null)
				_project = GetProjectFromFilename(_spanFactory, _segmentPool, ConfigFilename);
			else // Should never get here given checks above, but let's be safe and write the check anyway
				_project = GetProjectFromResource(_spanFactory, _segmentPool);
			_project.Meanings.Add(_meaning);
			_project.Varieties.Add(_variety);
		}

		public ReturnCodes DoWorkWithErrorChecking(TextReader inputReader, TextWriter outputWriter, TextWriter errorWriter)
		{
			ReturnCodes retcode = DoWork(inputReader, outputWriter, errorWriter);
			if (!warnings.Empty)
			{
				errorWriter.WriteLine("Operation \"{0}\" produced {1}:", GetVerbName(), CommandLineHelpers.CountedNoun(warnings.Count, "warning"));
				warnings.Write(errorWriter);
				// Do not change retcode for warnings
			}
			if (!errors.Empty)
			{
				errorWriter.WriteLine("Operation \"{0}\" produced {1}:", GetVerbName(), CommandLineHelpers.CountedNoun(errors.Count, "error"));
				errors.Write(errorWriter);
				retcode = (retcode == ReturnCodes.Okay) ? ReturnCodes.InputError : retcode;
			}
			return retcode;
		}

		public virtual ReturnCodes DoWork(TextReader inputReader, TextWriter outputWriter, TextWriter errorWriter)
		{
			return ReturnCodes.NotImplemented;
		}

		public ReturnCodes RunAsPipe()
		{
			return RunAsPipe(Console.Error);
		}

		public ReturnCodes RunAsPipe(TextWriter errorWriter)
		{
			try
			{
				using (StreamReader input = OpenInput())
				using (StreamWriter output = OpenOutput())
					return DoWorkWithErrorChecking(input, output, errorWriter);
			}
			catch (IOException exception)
			{
				// On Mono, piping into a closed process throws an IOException with "Write fault on path (something)"

				// Try to get the message in English, if we can (some exceptions are only localized when the .Message property is accessed)
				CultureInfo originalCultureInfo = Thread.CurrentThread.CurrentUICulture;
				Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
				string msg = exception.Message;
				Thread.CurrentThread.CurrentUICulture = originalCultureInfo;

				if (!msg.StartsWith("Write fault on path"))
					throw;
				return ReturnCodes.Okay; // Having our output pipe closed on us is not a real error
			}
		}

		protected Word ParseWord(string wordText, Meaning meaning)
		{
			int stemStartIdx = wordText.IndexOf("|", StringComparison.Ordinal);
			int stemEndIdx = wordText.LastIndexOf("|", StringComparison.Ordinal) - 1; // -1 because we're going to remove the leading |
			if (stemStartIdx != -1 && stemEndIdx < stemStartIdx)
			{
				// Only way this can happen is if there was only a single "|" in the word
				throw new FormatException(String.Format("Words should have either 0 or 2 pipe characters representing word stems. Offending word: {0}", wordText));
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

		public static CogProject GetProjectFromResource(SpanFactory<ShapeNode> spanFactory, SegmentPool segmentPool)
		{
			Stream stream = Assembly.GetAssembly(typeof(CommandLineHelpers)).GetManifestResourceStream("SIL.Cog.CommandLine.NewProject.cogx");
			return ConfigManager.Load(spanFactory, segmentPool, stream);
		}

		public static CogProject GetProjectFromFilename(SpanFactory<ShapeNode> spanFactory, SegmentPool segmentPool, string projectFilename)
		{
			if (projectFilename == null)
			{
				return GetProjectFromResource(spanFactory, segmentPool);
			}
			else
			{
				return ConfigManager.Load(spanFactory, segmentPool, projectFilename);
			}
		}

		public static CogProject GetProjectFromXmlString(SpanFactory<ShapeNode> spanFactory, SegmentPool segmentPool, string xmlString)
		{
			return ConfigManager.LoadFromXmlString(spanFactory, segmentPool, xmlString);
		}
	}
}
