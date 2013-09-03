using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
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
		private readonly SimpleMonitor _selectMonitor;

		public WordListsView()
		{
			InitializeComponent();
			WordListsGrid.ClipboardExporters.Clear();
			WordListsGrid.ClipboardExporters.Add(DataFormats.UnicodeText, new UnicodeCsvClipboardExporter {IncludeColumnHeaders = false, FormatSettings = {TextQualifier = '\0'}});
			_selectMonitor = new SimpleMonitor();
			BusyCursor.DisplayUntilIdle();
		}

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var vm = DataContext as WordListsViewModel;
			if (vm == null)
				return;

			vm.PropertyChanged += ViewModel_PropertyChanged;
			vm.Senses.CollectionChanged += Senses_CollectionChanged;
		}

		private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (IsVisible)
				Dispatcher.BeginInvoke(new Action(() => WordListsGrid.Focus()));
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			LoadCollectionView();
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
					DispatcherHelper.CheckBeginInvokeOnUI(LoadCollectionView);
					break;

				case "SelectedVarietySense":
					DispatcherHelper.CheckBeginInvokeOnUI(() =>
						{
							if (_selectMonitor.Busy)
								return;

							using (_selectMonitor.Enter())
							{
								WordListsGrid.SelectedCellRanges.Clear();
								if (vm.SelectedVarietySense != null)
								{
									WordListsVarietyViewModel variety = vm.SelectedVarietySense.Variety;
									int itemIndex = WordListsGrid.Items.IndexOf(variety);
									WordListsGrid.BringItemIntoView(variety);
									WordListsGrid.Dispatcher.BeginInvoke(new Action(() =>
									    {
									        var row = (DataRow) WordListsGrid.GetContainerFromIndex(itemIndex);
										    if (row != null)
										    {
											    Cell cell = row.Cells.Single(c => c.Content == vm.SelectedVarietySense);
												WordListsGrid.SelectedCellRanges.Add(new SelectionCellRange(itemIndex, cell.ParentColumn.Index));
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

			WordListsGrid.CurrentColumn = null;
			WordListsGrid.CurrentItem = null;
			WordListsGrid.Columns.Clear();
			var view = new DataGridCollectionView(vm.Varieties, typeof(WordListsVarietyViewModel), false, false);
			view.ItemProperties.Add(new DataGridItemProperty("Variety", ".", typeof(WordListsVarietyViewModel)));
			IComparer sortComparer = ProjectionComparer<WordListsVarietySenseViewModel>.Create(sense => sense.StrRep);
			for (int i = 0; i < vm.Senses.Count; i++)
				view.ItemProperties.Add(new DataGridItemProperty("Sense" + i, string.Format("Senses[{0}]", i), typeof(WordListsVarietySenseViewModel)) {SortComparer = sortComparer});
			vm.VarietiesView = view;
			WordListsGrid.Items.SortDescriptions.Clear();

			var headerColumn = new Column {FieldName = "Variety"};
			DataGridControlBehaviors.SetIsRowHeader(headerColumn, true);
			DataGridControlBehaviors.SetAutoSize(headerColumn, true);
			WordListsGrid.Columns.Add(headerColumn);
			for (int i = 0; i < vm.Senses.Count; i++)
			{
				var column = new Column {FieldName = "Sense" + i, Width = 100, CellEditor = WordListsGrid.DefaultCellEditors[typeof (WordListsVarietySenseViewModel)]};
				var titleBinding = new Binding(string.Format("DataGridControl.DataContext.Senses[{0}].Gloss", i)) {RelativeSource = RelativeSource.Self};
				BindingOperations.SetBinding(column, ColumnBase.TitleProperty, titleBinding);
				WordListsGrid.Columns.Add(column);
			}
		}

		private void Senses_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			LoadCollectionView();
		}

		private void WordListsGrid_OnSelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
		{
			var vm = (WordListsViewModel) DataContext;
			if (_selectMonitor.Busy)
				return;

			using (_selectMonitor.Enter())
			{
				if (WordListsGrid.SelectedCellRanges.Count == 1)
				{
					SelectionCellRange cellRange = WordListsGrid.SelectedCellRanges[0];
					int itemIndex = cellRange.ItemRange.StartIndex;
					var variety = (WordListsVarietyViewModel) WordListsGrid.Items[itemIndex];
					int columnIndex = cellRange.ColumnRange.StartIndex;
					vm.SelectedVarietySense = variety.Senses[columnIndex - 1];
				}
				else
				{
					vm.SelectedVarietySense = null;
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
