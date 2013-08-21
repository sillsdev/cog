using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Threading;
using SIL.Cog.Applications.ViewModels;
using SIL.Cog.Presentation.Behaviors;
using SIL.Collections;
using Xceed.Wpf.DataGrid;

namespace SIL.Cog.Presentation.Views
{
	/// <summary>
	/// Interaction logic for WordListsView.xaml
	/// </summary>
	public partial class WordListsView
	{
		private readonly SimpleMonitor _monitor;
		private InputBinding _findBinding;

		public WordListsView()
		{
			InitializeComponent();
			WordListsGrid.ClipboardExporters.Clear();
			WordListsGrid.ClipboardExporters.Add(DataFormats.UnicodeText, new UnicodeCsvClipboardExporter {IncludeColumnHeaders = false, FormatSettings = {TextQualifier = '\0'}});
			_monitor = new SimpleMonitor();
			BusyCursor.DisplayUntilIdle();
		}

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var vm = DataContext as WordListsViewModel;
			if (vm == null)
				return;

			vm.PropertyChanged += ViewModel_PropertyChanged;
			vm.Senses.CollectionChanged += Senses_CollectionChanged;
			vm.Varieties.CollectionChanged += Varieties_CollectionChanged;
			AddVarieties(vm.Varieties);
			_findBinding = new InputBinding(vm.FindCommand, new KeyGesture(Key.F, ModifierKeys.Control));
		}

		private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var window = this.FindVisualAncestor<Window>();
			if (IsVisible)
			{
				window.InputBindings.Add(_findBinding);
				Dispatcher.BeginInvoke(new Action(() => WordListsGrid.Focus()));
			}
			else
			{
				window.InputBindings.Remove(_findBinding);
			}
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			LoadCollectionView();
			WordListsGrid.SelectFirstCell();
		}

		private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var vm = (WordListsViewModel) sender;
			switch (e.PropertyName)
			{
				case "Senses":
					vm.Senses.CollectionChanged += Senses_CollectionChanged;
					break;

				case "Varieties":
					vm.Varieties.CollectionChanged += Varieties_CollectionChanged;
					AddVarieties(vm.Varieties);
					DispatcherHelper.CheckBeginInvokeOnUI(() =>
						{
							LoadCollectionView();
							WordListsGrid.SelectFirstCell();
						});
					break;

				case "CurrentVarietySense":
					DispatcherHelper.CheckBeginInvokeOnUI(() =>
						{
							if (_monitor.Busy)
								return;

							using (_monitor.Enter())
							{
								WordListsGrid.SelectedCellRanges.Clear();
								if (vm.CurrentVarietySense != null)
								{
									WordListsVarietyViewModel variety = vm.CurrentVarietySense.Variety;
									int itemIndex = vm.Varieties.IndexOf(variety);
									int columnIndex = variety.Senses.IndexOf(vm.CurrentVarietySense);
									WordListsGrid.SelectedCellRanges.Add(new SelectionCellRange(itemIndex, columnIndex));
									WordListsGrid.BringItemIntoView(variety);
									WordListsGrid.Dispatcher.BeginInvoke(new Action(() =>
									    {
									        var row = (DataRow) WordListsGrid.GetContainerFromIndex(itemIndex);
										    if (row != null)
										    {
											    Cell cell = row.Cells[columnIndex];
											    cell.BringIntoView();
										    }
									    }), DispatcherPriority.Background);
								}
							}
						});
					break;
			}
		}

		private void LoadCollectionView()
		{
			var vm = (WordListsViewModel) DataContext;
			var source = new DataGridCollectionView(vm.Varieties, typeof(WordListsVarietyViewModel), false, false);
			source.ItemProperties.Add(new DataGridItemProperty("Variety", ".", typeof(WordListsVarietyViewModel)) {IsReadOnly = true});
			IComparer sortComparer = ProjectionComparer<WordListsVarietySenseViewModel>.Create(sense => sense.StrRep);
			for (int i = 0; i < vm.Senses.Count; i++)
				source.ItemProperties.Add(new DataGridItemProperty(vm.Senses[i].Gloss, string.Format("Senses[{0}]", i), typeof(WordListsVarietySenseViewModel)) {SortComparer = sortComparer});
			WordListsGrid.ItemsSource = source;
			WordListsGrid.Items.SortDescriptions.Clear();

			WordListsGrid.Columns.Clear();
			var headerColumn = new Column {FieldName = "Variety"};
			DataGridControlBehaviors.SetIsRowHeader(headerColumn, true);
			WordListsGrid.Columns.Add(headerColumn);
			headerColumn.SetWidthToFit<WordListsVarietyViewModel>(v => v.Name, 18);
			foreach (SenseViewModel sense in vm.Senses)
				WordListsGrid.Columns.Add(new Column {FieldName = sense.Gloss, Title = sense.Gloss, Width = 100, CellEditor = WordListsGrid.DefaultCellEditors[typeof(WordListsVarietySenseViewModel)]});
		}

		private void Senses_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			LoadCollectionView();
		}

		private void Varieties_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					AddVarieties(e.NewItems.Cast<WordListsVarietyViewModel>());
					break;

				case NotifyCollectionChangedAction.Remove:
					RemoveVarieties(e.OldItems.Cast<WordListsVarietyViewModel>());
					break;

				case NotifyCollectionChangedAction.Replace:
					RemoveVarieties(e.OldItems.Cast<WordListsVarietyViewModel>());
					AddVarieties(e.NewItems.Cast<WordListsVarietyViewModel>());
					break;

				case NotifyCollectionChangedAction.Reset:
					AddVarieties(((IEnumerable) sender).Cast<WordListsVarietyViewModel>());
					break;
			}

			Dispatcher.BeginInvoke(new Action(() => WordListsGrid.Columns[0].SetWidthToFit<WordListsVarietyViewModel>(v => v.Name, 18)));
		}

		private void AddVarieties(IEnumerable<WordListsVarietyViewModel> varieties)
		{
			foreach (WordListsVarietyViewModel variety in varieties)
				variety.PropertyChanged += variety_PropertyChanged;
		}

		private void RemoveVarieties(IEnumerable<WordListsVarietyViewModel> varieties)
		{
			foreach (WordListsVarietyViewModel variety in varieties)
				variety.PropertyChanged -= variety_PropertyChanged;
		}

		private void variety_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "Name":
					DispatcherHelper.CheckBeginInvokeOnUI(() => WordListsGrid.Columns[0].SetWidthToFit<WordListsVarietyViewModel>(v => v.Name, 18));
					break;
			}
		}

		private void WordListsGrid_OnSelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
		{
			var vm = (WordListsViewModel) DataContext;
			if (_monitor.Busy)
				return;

			using (_monitor.Enter())
			{
				if (WordListsGrid.SelectedCellRanges.Count == 1)
				{
					SelectionCellRange cellRange = WordListsGrid.SelectedCellRanges[0];
					int itemIndex = cellRange.ItemRange.StartIndex;
					WordListsVarietyViewModel variety = vm.Varieties[itemIndex];
					int columnIndex = cellRange.ColumnRange.StartIndex;
					vm.CurrentVarietySense = variety.Senses[columnIndex];
				}
				else
				{
					vm.CurrentVarietySense = null;
				}
			}
		}

		private void Cell_OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
		{
			var cell = (DataCell) sender;
			WordListsGrid.SelectedCellRanges.Clear();
			int itemIndex = WordListsGrid.Items.IndexOf(cell.ParentRow.DataContext);
			WordListsGrid.SelectedCellRanges.Add(new SelectionCellRange(itemIndex, cell.ParentColumn.Index));
		}
	}
}
