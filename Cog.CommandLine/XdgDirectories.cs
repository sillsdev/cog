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
		private static readonly string AssemblyName = typeof(Program).Assembly.GetName().Name; // Currently "cog-cli"

		public static string Home => Environment.GetEnvironmentVariable("HOME") ?? "/home/nobody";

		public static string ConfigHome
		{
			get
			{
				string baseDir = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME") ?? Path.Combine(Home, DefaultConfigHome);
				return Path.Combine(baseDir, AssemblyName);
			}
		}

		public static string DataHome
		{
			get
			{
				string baseDir = Environment.GetEnvironmentVariable("XDG_DATA_HOME") ?? Path.Combine(Home, DefaultDataHome);
				return Path.Combine(baseDir, AssemblyName);
			}
		}

		public static string[] ConfigDirs
		{
			get
			{
				string xdgPathSpec = Environment.GetEnvironmentVariable("XDG_CONFIG_DIRS");
				string[] baseDirs = xdgPathSpec?.Split(':') ?? DefaultConfigDirs;
				return baseDirs.Select(dir => Path.Combine(dir, AssemblyName)).ToArray();
			}
		}

		public static string[] DataDirs
		{
			get
			{
				string xdgPathSpec = Environment.GetEnvironmentVariable("XDG_DATA_DIRS");
				string[] baseDirs = xdgPathSpec?.Split(':') ?? DefaultDataDirs;
				return baseDirs.Select(dir => Path.Combine(dir, AssemblyName)).ToArray();	
			}
		}
	}
}
