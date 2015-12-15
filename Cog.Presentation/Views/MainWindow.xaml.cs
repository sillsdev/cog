using System;
using System.ComponentModel;
using System.Windows;
using SIL.Cog.Application.ViewModels;
using SIL.Cog.Presentation.Properties;

namespace SIL.Cog.Presentation.Views
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

		protected override void OnClosing(CancelEventArgs e)
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
			base.OnClosing(e);
		}

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);
			WindowPlacement.SetPlacement(this, Settings.Default.WindowPlacement);
		}
	}
}
