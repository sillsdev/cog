using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
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
			vm.VarietiesView.CollectionChanged += VarietiesView_CollectionChanged;
			AddVarieties(vm.VarietiesView.Cast<VarietiesVarietyViewModel>());
			VarietiesComboBox.SetWidthToFit<VarietiesVarietyViewModel>(variety => variety.Name);
		}

		private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "Varieties":
					DispatcherHelper.CheckBeginInvokeOnUI(SetupVarieties);
					break;

				case "CurrentVariety":
					BusyCursor.DisplayUntilIdle();
					break;
			}
		}

		private void VarietiesView_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					AddVarieties(e.NewItems.Cast<VarietiesVarietyViewModel>());
					break;

				case NotifyCollectionChangedAction.Remove:
					RemoveVarieties(e.OldItems.Cast<VarietiesVarietyViewModel>());
					break;

				case NotifyCollectionChangedAction.Replace:
					RemoveVarieties(e.OldItems.Cast<VarietiesVarietyViewModel>());
					AddVarieties(e.NewItems.Cast<VarietiesVarietyViewModel>());
					break;

				case NotifyCollectionChangedAction.Reset:
					AddVarieties(((IEnumerable) sender).Cast<VarietiesVarietyViewModel>());
					break;
			}
			VarietiesComboBox.SetWidthToFit<VarietiesVarietyViewModel>(variety => variety.Name);
		}

		private void AddVarieties(IEnumerable<VarietiesVarietyViewModel> varieties)
		{
			foreach (VarietiesVarietyViewModel variety in varieties)
				variety.PropertyChanged += variety_PropertyChanged;
		}

		private void RemoveVarieties(IEnumerable<VarietiesVarietyViewModel> varieties)
		{
			foreach (VarietiesVarietyViewModel variety in varieties)
				variety.PropertyChanged -= variety_PropertyChanged;
		}

		private void variety_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "Name":
					DispatcherHelper.CheckBeginInvokeOnUI(() => VarietiesComboBox.SetWidthToFit<VarietiesVarietyViewModel>(variety => variety.Name));
					break;
			}
		}
	}
}
