using System.ComponentModel;
using System.Windows;
using GalaSoft.MvvmLight.Threading;
using SIL.Cog.Application.ViewModels;

namespace SIL.Cog.Presentation.Views
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
			vm.PropertyChanged += vm_PropertyChanged;
			vm.Execute();
		}

		private void vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "Executing":
					DispatcherHelper.CheckBeginInvokeOnUI(() =>
						{
							var vm = (ProgressViewModel) DataContext;
							if (IsLoaded && !vm.Executing)
								DialogResult = !vm.Canceled;
						});
					break;
			}
		}
	}
}
