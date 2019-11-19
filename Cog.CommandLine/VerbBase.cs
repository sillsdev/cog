using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using CommandLine;
using CommandLine.Text;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Config;

namespace SIL.Cog.CommandLine
{
	public class VerbBase // Non-abstract so we can unit test it
	{
		[Option('i', "input", Default = "-", HelpText = "Input filename (\"-\" for stdin)")]
		public string InputFilename { get; set; }

		[Option('o', "output", Default = "-", HelpText = "Output filename (\"-\" for stdout)")]
		public string OutputFilename { get; set; }

		[Option('c', "config-file",
			HelpText = "Config file to use instead of default config (--config-data will override this, if passed)")]
		public string ConfigFilename { get; set; }

		[Option("config-data", HelpText = "Configuration to use, as a single long string (if passed, overrides --config-file)")]
		public string ConfigData { get; set; }

		[Usage(ApplicationAlias = "cog-cli")]
		public static IEnumerable<Example> Examples
		{
			get
			{
				yield return new Example("Specify a config file on the command line",
					new VerbBase {ConfigFilename = "cog-cli.conf"});
				yield return new Example("Read/write from files instead of stdin/out",
					new VerbBase {InputFilename = "infile.txt", OutputFilename = "outfile.txt"});
			}
		}

		public SegmentPool SegmentPool { get; set; }
		public CogProject Project { get; set; }
		public Errors Errors { get; set; } = new Errors();
		public Errors Warnings { get; set; } = new Errors();

		public virtual string GetVerbName()
		{
			foreach (VerbAttribute attrib in GetType().GetCustomAttributes(typeof(VerbAttribute), true))
			{
				return attrib.Name;
			}
			return string.Empty;
		}

		protected IEnumerable<string> PossibleConfigFilenames
		{ 
			get
			{
				if (!Platform.IsMono)
					yield break;

				// We're slightly violating the XDG spec here: we look for the user's config in $XDG_CONFIG_HOME, but if
				// he doesn't have a config file there, we get the default from $XDG_DATA_DIRS rather than $XDG_CONFIG_DIRS.
				string assemblyName = typeof(Program).Assembly.GetName().Name;
				string configFileName = assemblyName + ".conf";
				yield return Path.Combine(XdgDirectories.ConfigHome, configFileName);
				foreach (string baseDir in XdgDirectories.DataDirs)
					yield return Path.Combine(baseDir, configFileName);
			}
		}

		protected string FindConfigFilename()
		{
			// Note that we can't validate the config files due to the Mono XSD validation bug,
			// so we just look for the first one that exists.
			foreach (string candidateFilename in PossibleConfigFilenames)
				if (File.Exists(candidateFilename))
					return candidateFilename;
			return null;
		}

		internal void SetupProject() // Should this be protected? But the unit test needs to call it.
		{
			if (ConfigData != null && ConfigFilename != null)
			{
				Warnings.Add("WARNING: options --config-data and --config-file were both specified. Ignoring --config-file.");
				ConfigFilename = null;
			}
			if (ConfigData == null && ConfigFilename == null)
			{
				ConfigFilename = FindConfigFilename();
				// If ConfigFilename is STILL null at this point, it's because no config files were found at all,
				// so we'll use the default one from the resource.
			}
			SegmentPool = new SegmentPool();
			if (ConfigData == null && ConfigFilename == null)
				Project = GetProjectFromResource(SegmentPool);
			else if (ConfigData != null)
				Project = GetProjectFromXmlString(SegmentPool, ConfigData);
			else if (ConfigFilename != null)
				Project = GetProjectFromFilename(SegmentPool, ConfigFilename);
			else // Should never get here given checks above, but let's be safe and write the check anyway
				Project = GetProjectFromResource(SegmentPool);
		}

		public ReturnCode DoWorkWithErrorChecking(TextReader inputReader, TextWriter outputWriter, TextWriter errorWriter)
		{
			ReturnCode retcode = DoWork(inputReader, outputWriter, errorWriter);
			if (!Warnings.Empty)
			{
				errorWriter.WriteLine("Operation \"{0}\" produced {1}:", GetVerbName(), CountedNoun(Warnings.Count, "warning"));
				Warnings.Write(errorWriter);
				// Do not change retcode for warnings
			}
			if (!Errors.Empty)
			{
				errorWriter.WriteLine("Operation \"{0}\" produced {1}:", GetVerbName(), CountedNoun(Errors.Count, "error"));
				Errors.Write(errorWriter);
				retcode = retcode == ReturnCode.Okay ? ReturnCode.InputError : retcode;
			}
			return retcode;
		}

		protected virtual ReturnCode DoWork(TextReader inputReader, TextWriter outputWriter, TextWriter errorWriter)
		{
			return ReturnCode.NotImplemented;
		}

		public ReturnCode RunAsPipe()
		{
			return RunAsPipe(Console.Error);
		}

		public ReturnCode RunAsPipe(TextWriter errorWriter)
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

				// Try to get the message in English,
				// if we can (some exceptions are only localized when the .Message property is accessed)
				CultureInfo originalCultureInfo = Thread.CurrentThread.CurrentUICulture;
				Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
				string msg = exception.Message;
				Thread.CurrentThread.CurrentUICulture = originalCultureInfo;

				if (!msg.StartsWith("Write fault on path"))
					throw;
				return ReturnCode.Okay; // Having our output pipe closed on us is not a real error
			}
		}

		protected Word ParseWord(string wordText, Meaning meaning)
		{
			int stemStartIdx = wordText.IndexOf("|", StringComparison.Ordinal);
			int stemEndIdx = wordText.LastIndexOf("|", StringComparison.Ordinal) - 1; // -1 because we're going to remove the leading |
			if (stemStartIdx != -1 && stemEndIdx < stemStartIdx)
			{
				// Only way this can happen is if there was only a single "|" in the word
				throw new FormatException($"Words should have either 0 or 2 pipe characters representing word stems. Offending word: {wordText}");
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

		public static CogProject GetProjectFromResource(SegmentPool segmentPool)
		{
			using (Stream stream = Assembly.GetAssembly(typeof(Program)).GetManifestResourceStream("SIL.Cog.CommandLine.DefaultProject.cogx"))
				return ConfigManager.Load(segmentPool, stream);
		}

		public static CogProject GetProjectFromFilename(SegmentPool segmentPool, string projectFilename)
		{
			if (projectFilename == null)
				return GetProjectFromResource(segmentPool);
			return ConfigManager.Load(segmentPool, projectFilename);
		}

		public static CogProject GetProjectFromXmlString(SegmentPool segmentPool, string xmlString)
		{
			return ConfigManager.LoadFromXmlString(segmentPool, xmlString);
		}

		public static string CountedNoun(int count, string singular)
		{
			return CountedNoun(count, singular, singular + "s");
		}

		public static string CountedNoun(int count, string singular, string plural)
		{
			return $"{count} {(count == 1 ? singular : plural)}";
		}

		public static IEnumerable<string> ReadLines(TextReader input)
		{
			string line;
			while ((line = input.ReadLine()) != null)
			{
				if (!string.IsNullOrWhiteSpace(line)) // Silently skip blank lines
					yield return line;
			}
		}

		protected IEnumerable<Tuple<string, string>> AllPossiblePairs(IEnumerable<string> words)
		{
			var queue = new Queue<string>(words);
			while (queue.Count > 0) // This is O(1), because Queue<T> keeps track of its count
			{
				string first = queue.Dequeue();
				foreach (string second in queue)
					yield return new Tuple<string, string>(first, second);
			}
		}
	}
}
