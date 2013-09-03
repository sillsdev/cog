using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using GalaSoft.MvvmLight.Threading;
using SIL.Cog.Applications.ViewModels;

namespace SIL.Cog.Presentation.Views
{
	/// <summary>
	/// Interaction logic for VarietiesView.xaml
	/// </summary>
	public partial class VarietiesView
	{
		public VarietiesView()
		{
			InitializeComponent();
			BusyCursor.DisplayUntilIdle();
		}

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var vm = DataContext as VarietiesViewModel;
			if (vm == null)
				return;

			vm.PropertyChanged += ViewModel_PropertyChanged;
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			SetupVarieties();
		}

		private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (IsVisible)
				Dispatcher.BeginInvoke(new Action(() => VarietiesComboBox.Focus()));
		}

		private void SetupVarieties()
		{
			var vm = (VarietiesViewModel) DataContext;
			vm.VarietiesView = CollectionViewSource.GetDefaultView(vm.Varieties);
		}

		private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "Varieties":
					DispatcherHelper.CheckBeginInvokeOnUI(SetupVarieties);
					break;

				case "SelectedVariety":
					BusyCursor.DisplayUntilIdle();
					break;
			}
		}
	}
}
