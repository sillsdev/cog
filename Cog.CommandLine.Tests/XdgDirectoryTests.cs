using System;
using System.IO;
using NUnit.Framework;

namespace SIL.Cog.CommandLine.Tests
{
	[TestFixture]
	[Platform("Linux")]
	public class XdgDirectoryTests
	{
		private const string ExpectedAssemblyName = "cog-cmdline";

		[Test]
		public void CheckHome()
		{
			Assert.That(XdgDirectories.Home, Is.Not.Null);
			// Last is default value on Windows
			Assert.That(XdgDirectories.Home, Is.EqualTo(Environment.GetEnvironmentVariable("HOME")) | Is.EqualTo("/home/nobody"));
		}

		[Test]
		public void CheckConfigHome()
		{
			string home = XdgDirectories.Home;
			Assert.That(XdgDirectories.ConfigHome, Is.EqualTo(Path.Combine(home, ".config", ExpectedAssemblyName)));
		}

		[Test]
		public void CheckDataHome()
		{
			string home = XdgDirectories.Home;
			Assert.That(XdgDirectories.DataHome, Is.EqualTo(Path.Combine(home, ".local/share", ExpectedAssemblyName)));
		}

		[Test]
		public void CheckConfigDirs()
		{
			string[] expectedDirs = {Path.Combine("/etc/xdg", ExpectedAssemblyName)};
			Assert.That(expectedDirs, Is.SubsetOf(XdgDirectories.ConfigDirs));
		}

		[Test]
		public void CheckDataDirs()
		{
			string[] expectedDirs =
			{
				Path.Combine("/usr/local/share", ExpectedAssemblyName),
				Path.Combine("/usr/share", ExpectedAssemblyName)
			};
			Assert.That(expectedDirs, Is.SubsetOf(XdgDirectories.DataDirs));
		}
	}
}
