using System;
using System.Windows;
using System.Windows.Threading;
using SIL.Cog.ViewModels;

namespace SIL.Cog.Views
{
	/// <summary>
	/// Interaction logic for ProgressDialog.xaml
	/// </summary>
	public partial class ProgressDialog
	{
		public ProgressDialog()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			var vm = (ProgressViewModel) DataContext;
			vm.Execute();
		}

		private void _progressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (IsLoaded && _progressBar.Value >= _progressBar.Maximum)
			{
				Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => DialogResult = true));
			}
		}
	}
}
