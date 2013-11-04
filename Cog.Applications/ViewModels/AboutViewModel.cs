using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace SIL.Cog.Applications.ViewModels
{
	public class AboutViewModel : ViewModelBase
	{
		private readonly ICommand _licenseCommand;
		private readonly ICommand _websiteCommand;
		private readonly string _version;
		private readonly DateTime _buildDate;

		public AboutViewModel()
		{
			_licenseCommand = new RelayCommand(() => Process.Start("http://sil.mit-license.org"));
			_websiteCommand = new RelayCommand(() => Process.Start("http://sillsdev.github.io/cog/"));

			_version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

			string filePath = Assembly.GetExecutingAssembly().Location;
			const int peHeaderOffset = 60;
			const int linkerTimestampOffset = 8;
			var b = new byte[2048];
			Stream s = null;
			try
			{
				s = new FileStream(filePath, FileMode.Open, FileAccess.Read);
				s.Read(b, 0, 2048);
			}
			finally
			{
				if (s != null)
				{
					s.Close();
				}
			}

			int i = BitConverter.ToInt32(b, peHeaderOffset);
			int secondsSince1970 = BitConverter.ToInt32(b, i + linkerTimestampOffset);
			_buildDate = new DateTime(1970, 1, 1, 0, 0, 0);
			_buildDate = _buildDate.AddSeconds(secondsSince1970);
			_buildDate = _buildDate.AddHours(TimeZone.CurrentTimeZone.GetUtcOffset(_buildDate).Hours);
		}

		public string Version
		{
			get { return _version; }
		}

		public DateTime BuildDate
		{
			get { return _buildDate; }
		}

		public ICommand LicenseCommand
		{
			get { return _licenseCommand; }
		}

		public ICommand WebsiteCommand
		{
			get { return _websiteCommand; }
		}
	}
}
