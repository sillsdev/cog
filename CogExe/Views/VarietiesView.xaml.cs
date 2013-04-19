using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
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
			SetupVarieties(vm);
		}

		private void SetupVarieties(VarietiesViewModel vm)
		{
			vm.Varieties.CollectionChanged += Varieties_CollectionChanged;
			AddVarieties(vm.Varieties);
			ViewUtilities.SetComboBoxWidthToFit<VarietyVarietiesViewModel>(VarietiesComboBox, variety => variety.Name);
		}

		private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Varieties")
			{
				var vm = (VarietiesViewModel) DataContext;
				SetupVarieties(vm);
			}
		}

		private void Varieties_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					AddVarieties(e.NewItems.Cast<VarietyVarietiesViewModel>());
					break;

				case NotifyCollectionChangedAction.Remove:
					RemoveVarieties(e.OldItems.Cast<VarietyVarietiesViewModel>());
					break;

				case NotifyCollectionChangedAction.Replace:
					RemoveVarieties(e.OldItems.Cast<VarietyVarietiesViewModel>());
					AddVarieties(e.NewItems.Cast<VarietyVarietiesViewModel>());
					break;

				case NotifyCollectionChangedAction.Reset:
					AddVarieties((IEnumerable<VarietyVarietiesViewModel>) sender);
					break;
			}
			ViewUtilities.SetComboBoxWidthToFit<VarietyVarietiesViewModel>(VarietiesComboBox, variety => variety.Name);
		}

		private void AddVarieties(IEnumerable<VarietyVarietiesViewModel> varieties)
		{
			foreach (VarietyVarietiesViewModel variety in varieties)
				variety.PropertyChanged += variety_PropertyChanged;
		}

		private void RemoveVarieties(IEnumerable<VarietyVarietiesViewModel> varieties)
		{
			foreach (VarietyVarietiesViewModel variety in varieties)
				variety.PropertyChanged -= variety_PropertyChanged;
		}

		private void variety_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Name")
				ViewUtilities.SetComboBoxWidthToFit<VarietyVarietiesViewModel>(VarietiesComboBox, variety => variety.Name);
		}
	}
}
