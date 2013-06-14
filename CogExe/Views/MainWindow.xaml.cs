using System;
using System.ComponentModel;
using System.Windows;
using SIL.Cog.Properties;
using SIL.Cog.ViewModels;

namespace SIL.Cog.Views
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void Exit_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			var vm = (MainWindowViewModel) DataContext;
			if (vm.CanExit())
			{
				Settings.Default.WindowPlacement = WindowPlacement.GetPlacement(this);
				Settings.Default.Save();
			}
			else
			{
				e.Cancel = true;
			}
		}

		private void AboutBox_Click(object sender, RoutedEventArgs e)
		{
			var aboutBox = new AboutBox(this);
			aboutBox.ShowDialog();
		}

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);
			WindowPlacement.SetPlacement(this, Settings.Default.WindowPlacement);
		}
	}
}
