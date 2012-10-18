using System.ComponentModel;
using System.Windows;
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
			if (vm.ExitCommand.CanExecute(null))
			{
				vm.ExitCommand.Execute(null);
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
	}
}
