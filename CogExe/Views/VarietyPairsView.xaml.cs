using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using GalaSoft.MvvmLight.Threading;
using SIL.Cog.ViewModels;

namespace SIL.Cog.Views
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

		private void VarietyPairsView_OnLoaded(object sender, RoutedEventArgs e)
		{
			var vm = (VarietyPairsViewModel) DataContext;
			vm.PropertyChanged += ViewModel_PropertyChanged;
			SetupVarieties();
		}

		private void SetupVarieties()
		{
			var vm = (VarietyPairsViewModel) DataContext;
			vm.Varieties.CollectionChanged += Varieties_CollectionChanged;
			AddVarieties(vm.Varieties);
			Varieties1ComboBox.SetWidthToFit<VarietyViewModel>(variety => variety.Name);
			Varieties2ComboBox.SetWidthToFit<VarietyViewModel>(variety => variety.Name);
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
					AddVarieties(e.NewItems.Cast<VarietyViewModel>());
					break;

				case NotifyCollectionChangedAction.Remove:
					RemoveVarieties(e.OldItems.Cast<VarietyViewModel>());
					break;

				case NotifyCollectionChangedAction.Replace:
					RemoveVarieties(e.OldItems.Cast<VarietyViewModel>());
					AddVarieties(e.NewItems.Cast<VarietyViewModel>());
					break;

				case NotifyCollectionChangedAction.Reset:
					AddVarieties((IEnumerable<VarietyViewModel>) sender);
					break;
			}
			Varieties1ComboBox.SetWidthToFit<VarietyViewModel>(variety => variety.Name);
			Varieties2ComboBox.SetWidthToFit<VarietyViewModel>(variety => variety.Name);
		}

		private void AddVarieties(IEnumerable<VarietyViewModel> varieties)
		{
			foreach (VarietyViewModel variety in varieties)
				variety.PropertyChanged += variety_PropertyChanged;
		}

		private void RemoveVarieties(IEnumerable<VarietyViewModel> varieties)
		{
			foreach (VarietyViewModel variety in varieties)
				variety.PropertyChanged -= variety_PropertyChanged;
		}

		private void variety_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Name")
			{
				DispatcherHelper.CheckBeginInvokeOnUI(() =>
					{
						Varieties1ComboBox.SetWidthToFit<VarietiesVarietyViewModel>(variety => variety.Name);
						Varieties2ComboBox.SetWidthToFit<VarietiesVarietyViewModel>(variety => variety.Name);
					});
			}
		}
	}
}
