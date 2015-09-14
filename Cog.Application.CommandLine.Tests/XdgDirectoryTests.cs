using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SIL.Cog.Application.CommandLine;

namespace SIL.Cog.Application.CommandLine.Tests
{
	[TestFixture]
	[Category("LinuxOnly")]
	public class XdgDirectoryTests
	{
		private const string ExpectedAssemblyName = "cog-cmdline";

		[Test]
		public void CheckHome()
		{
			Assert.That(XdgDirectories.Home, Is.Not.Null);
			Assert.That(XdgDirectories.Home, Is.EqualTo(Environment.GetEnvironmentVariable("HOME")) | Is.EqualTo("/home/nobody")); // Last is default value on Windows
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
			var ExpectedDirs = new string[] { Path.Combine("/etc/xdg", ExpectedAssemblyName) };
			Assert.That(ExpectedDirs, Is.SubsetOf(XdgDirectories.ConfigDirs));
		}

		[Test]
		public void CheckDataDirs()
		{
			var ExpectedDirs = new string[] { Path.Combine("/usr/local/share", ExpectedAssemblyName), Path.Combine("/usr/share", ExpectedAssemblyName) };
			Assert.That(ExpectedDirs, Is.SubsetOf(XdgDirectories.DataDirs));
		}
	}
}
