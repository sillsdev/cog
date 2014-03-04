using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using GalaSoft.MvvmLight.Threading;
using SIL.Cog.Application.ViewModels;

namespace SIL.Cog.Presentation.Views
{
	/// <summary>
	/// Interaction logic for VarietyPairsView.xaml
	/// </summary>
	public partial class VarietyPairsView
	{
		public VarietyPairsView()
		{
			InitializeComponent();
			BusyCursor.DisplayUntilIdle();
		}

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var vm = DataContext as VarietyPairsViewModel;
			if (vm == null)
				return;

			vm.PropertyChanged += ViewModel_PropertyChanged;
		}

		private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (IsVisible)
				Dispatcher.BeginInvoke(new Action(() => Varieties1ComboBox.Focus()));
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			SetupVarieties();
		}

		private void SetupVarieties()
		{
			var vm = (VarietyPairsViewModel) DataContext;

			vm.VarietiesView1 = new ListCollectionView(vm.Varieties);
			vm.VarietiesView2 = new ListCollectionView(vm.Varieties);
		}

		private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "Varieties":
					DispatcherHelper.CheckBeginInvokeOnUI(SetupVarieties);
					break;
			}
		}
	}
}
