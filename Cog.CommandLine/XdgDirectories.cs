using System;
using System.IO;
using System.Linq;

namespace SIL.Cog.CommandLine
{
	public static class XdgDirectories
	{
		private static readonly string DefaultConfigHome = ".config";
		private static readonly string DefaultDataHome = ".local/share";
		private static readonly string[] DefaultConfigDirs = { "/etc/xdg" };
		private static readonly string[] DefaultDataDirs = { "/usr/local/share", "/usr/share" };
		private static readonly string AssemblyName = typeof(Program).Assembly.GetName().Name; // Currently "cog-cmdline"

		public static string Home
		{
			get { return Environment.GetEnvironmentVariable("HOME") ?? "/home/nobody"; } // That last is needed so we can run unit tests on Windows machines
		}

		public static string ConfigHome
		{
			get
			{
				string BaseDir = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME") ?? Path.Combine(Home, DefaultConfigHome);
				return Path.Combine(BaseDir, AssemblyName);
			}
		}

		public static string DataHome
		{
			get
			{
				string BaseDir = Environment.GetEnvironmentVariable("XDG_DATA_HOME") ?? Path.Combine(Home, DefaultDataHome);
				return Path.Combine(BaseDir, AssemblyName);
			}
		}

		public static string[] ConfigDirs
		{
			get
			{
				string XdgPathSpec = Environment.GetEnvironmentVariable("XDG_CONFIG_DIRS");
				string[] BaseDirs = (XdgPathSpec == null) ? DefaultConfigDirs : XdgPathSpec.Split(':');
				return BaseDirs.Select(dir => Path.Combine(dir, AssemblyName)).ToArray();
			}
		}

		public static string[] DataDirs
		{
			get
			{
				string XdgPathSpec = Environment.GetEnvironmentVariable("XDG_DATA_DIRS");
				string[] BaseDirs = (XdgPathSpec == null) ? DefaultDataDirs : XdgPathSpec.Split(':');
				return BaseDirs.Select(dir => Path.Combine(dir, AssemblyName)).ToArray();	
			}
		}
	}
}
