using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.Threading;
using SIL.Cog.Applications.ViewModels;

namespace SIL.Cog.Presentation.Views
{
	/// <summary>
	/// Interaction logic for VarietyPairsView.xaml
	/// </summary>
	public partial class VarietyPairsView
	{
		private InputBinding _findBinding;

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
			_findBinding = new InputBinding(vm.FindCommand, new KeyGesture(Key.F, ModifierKeys.Control));
		}

		private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var window = this.FindVisualAncestor<Window>();
			if (IsVisible)
				window.InputBindings.Add(_findBinding);
			else
				window.InputBindings.Remove(_findBinding);
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			SetupVarieties();
			SetupVarietiesView1();
			SetupVarietiesView2();
		}

		private void SetupVarieties()
		{
			var vm = (VarietyPairsViewModel) DataContext;
			vm.Varieties.CollectionChanged += Varieties_CollectionChanged;
			AddVarieties(vm.Varieties);
		}

		private void SetupVarietiesView1()
		{
			var vm = (VarietyPairsViewModel) DataContext;
			vm.VarietiesView1.CollectionChanged += (sender, e) => Varieties1ComboBox.SetWidthToFit<VarietyViewModel>(variety => variety.Name);
			Varieties1ComboBox.SetWidthToFit<VarietyViewModel>(variety => variety.Name);
		}

		private void SetupVarietiesView2()
		{
			var vm = (VarietyPairsViewModel) DataContext;
			vm.VarietiesView2.CollectionChanged += (sender, e) => Varieties2ComboBox.SetWidthToFit<VarietyViewModel>(variety => variety.Name);
			Varieties2ComboBox.SetWidthToFit<VarietyViewModel>(variety => variety.Name);
		}

		private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "Varieties":
					DispatcherHelper.CheckBeginInvokeOnUI(SetupVarieties);
					break;

				case "VarietiesView1":
					DispatcherHelper.CheckBeginInvokeOnUI(SetupVarietiesView1);
					break;

				case "VarietiesView2":
					DispatcherHelper.CheckBeginInvokeOnUI(SetupVarietiesView2);
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
