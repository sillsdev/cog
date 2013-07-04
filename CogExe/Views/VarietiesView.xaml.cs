﻿using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using GalaSoft.MvvmLight.Threading;
using SIL.Cog.ViewModels;

namespace SIL.Cog.Views
{
	/// <summary>
	/// Interaction logic for VarietiesView.xaml
	/// </summary>
	public partial class VarietiesView
	{
		public VarietiesView()
		{
			InitializeComponent();
		}

		private void VarietiesView_OnLoaded(object sender, RoutedEventArgs e)
		{
			var vm = (VarietiesViewModel) DataContext;
			vm.PropertyChanged += ViewModel_PropertyChanged;
			SetupVarieties();
		}

		private void SetupVarieties()
		{
			var vm = (VarietiesViewModel) DataContext;
			vm.Varieties.CollectionChanged += Varieties_CollectionChanged;
			AddVarieties(vm.Varieties);
			VarietiesComboBox.SetWidthToFit<VarietiesVarietyViewModel>(variety => variety.Name);
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

		private void Varieties_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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
					AddVarieties((IEnumerable<VarietiesVarietyViewModel>) sender);
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
