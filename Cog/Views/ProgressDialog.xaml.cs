using System.ComponentModel;
using System.Windows;
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
			vm.PropertyChanged += ViewModel_PropertyChanged;
			vm.Execute();
		}

		private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var vm = (ProgressViewModel) sender;
			switch (e.PropertyName)
			{
				case "IsCompleted":
					if (vm.IsCompleted)
						DialogResult = true;
					break;
			}
		}
	}
}
