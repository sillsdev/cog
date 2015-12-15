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

		[Usage(ApplicationAlias = "cog-cmdline")]
		public static IEnumerable<Example> Examples
		{
			get
			{
				yield return new Example("Specify a config file on the command line", new VerbBase { ConfigFilename = "cog-cmdline.conf" } );
				yield return new Example("Read/write from files instead of stdin/out", new VerbBase { InputFilename = "infile.txt", OutputFilename = "outfile.txt" } );
			}
		}

		protected SegmentPool _segmentPool;
		protected SpanFactory<ShapeNode> _spanFactory;
		protected CogProject _project;
		protected Meaning _meaning;
		protected Variety _variety1;
		protected Variety _variety2;

		public SegmentPool SegmentPool
		{
			get { return _segmentPool; }
			set { _segmentPool = value; }
		}

		public SpanFactory<ShapeNode> SpanFactory
		{
			get { return _spanFactory; }
			set { _spanFactory = value; }
		}

		public CogProject Project
		{
			get { return _project; }
			set { _project = value; }
		}

		public Variety Variety1
		{
			get { return _variety1; }
			set { _variety1 = value; }
		}

		public Variety Variety2
		{
			get { return _variety2; }
			set { _variety2 = value; }
		}

		public Meaning Meaning
		{
			get { return _meaning; }
			set { _meaning = value; }
		}

		private Errors _errors = new Errors();
		private Errors _warnings = new Errors();

		public Errors Errors
		{
			get { return _errors; }
			set { _errors = value; }
		}

		public Errors Warnings
		{
			get { return _warnings; }
			set { _warnings = value; }
		}


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
				Warnings.Add("WARNING: options --config-data and --config-file were both specified. Ignoring --config-file.");
				ConfigFilename = null;
			}
			if (ConfigData == null && ConfigFilename == null)
			{
				ConfigFilename = FindConfigFilename();
				// If ConfigFilename is STILL null at this point, it's because no config files were found at all, so we'll use the default one from the resource.
			}
			SpanFactory = new ShapeSpanFactory();
			SegmentPool = new SegmentPool();
			Variety1 = new Variety("variety1");
			Variety2 = new Variety("variety2");
			Meaning = new Meaning("gloss1", "cat1");
			if (ConfigData == null && ConfigFilename == null)
				Project = GetProjectFromResource(SpanFactory, SegmentPool);
			else if (ConfigData != null)
				Project = GetProjectFromXmlString(SpanFactory, SegmentPool, ConfigData);
			else if (ConfigFilename != null)
				Project = GetProjectFromFilename(SpanFactory, SegmentPool, ConfigFilename);
			else // Should never get here given checks above, but let's be safe and write the check anyway
				Project = GetProjectFromResource(SpanFactory, SegmentPool);
			Project.Meanings.Add(Meaning);
			Project.Varieties.Add(Variety1);
			Project.Varieties.Add(Variety2);
			Project.VarietyPairs.Add(new VarietyPair(Variety1, Variety2));
		}

		public ReturnCodes DoWorkWithErrorChecking(TextReader inputReader, TextWriter outputWriter, TextWriter errorWriter)
		{
			ReturnCodes retcode = DoWork(inputReader, outputWriter, errorWriter);
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
				retcode = (retcode == ReturnCodes.Okay) ? ReturnCodes.InputError : retcode;
			}
			return retcode;
		}

		protected virtual ReturnCodes DoWork(TextReader inputReader, TextWriter outputWriter, TextWriter errorWriter)
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
			using (Stream stream = Assembly.GetAssembly(typeof(Program)).GetManifestResourceStream("SIL.Cog.CommandLine.NewProject.cogx"))
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

		public static string CountedNoun(int count, string singular)
		{
			return CountedNoun(count, singular, singular + "s");
		}

		public static string CountedNoun(int count, string singular, string plural)
		{
			return String.Format("{0} {1}", count, count == 1 ? singular : plural);
		}

		public static IEnumerable<string> ReadLines(TextReader input)
		{
			string line;
			while ((line = input.ReadLine()) != null)
			{
				if (!String.IsNullOrWhiteSpace(line)) // Silently skip blank lines
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
