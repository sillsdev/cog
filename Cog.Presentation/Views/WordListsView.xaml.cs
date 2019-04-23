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
using SIL.Cog.Application.ViewModels;
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
			if (!DesignerProperties.GetIsInDesignMode(this))
				BusyCursor.DisplayUntilIdle();
		}

		private Cell ActiveCell
		{
			get
			{
				if (WordListsGrid.CurrentColumn == null)
					return null;
				var currentRow = WordListsGrid.GetContainerFromItem(WordListsGrid.CurrentItem) as Row;
				if (currentRow == null)
					return null;
				return currentRow.Cells[WordListsGrid.CurrentColumn.Index];
			}
		}

		private void SelectActiveCell()
		{
			WordListsGrid.SelectedCellRanges.Clear();
			WordListsGrid.SelectedCellRanges.Add(new SelectionCellRange(WordListsGrid.Items.IndexOf(WordListsGrid.CurrentItem), WordListsGrid.CurrentColumn.Index));
		}

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var vm = DataContext as WordListsViewModel;
			if (vm == null)
				return;

			vm.PropertyChanged += ViewModel_PropertyChanged;
			if (vm.Meanings != null)
				vm.Meanings.CollectionChanged += Meanings_CollectionChanged;
		}

		private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (IsVisible)
				Dispatcher.BeginInvoke(new Action(() => WordListsGrid.Focus()));
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			if (!DesignerProperties.GetIsInDesignMode(this))
				LoadCollectionView();
		}

		private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var vm = (WordListsViewModel) sender;
			switch (e.PropertyName)
			{
				case "Meanings":
					vm.Meanings.CollectionChanged += Meanings_CollectionChanged;
					break;

				case "Varieties":
					DispatcherHelper.CheckBeginInvokeOnUI(LoadCollectionView);
					break;

				case "SelectedVarietyMeaning":
					DispatcherHelper.CheckBeginInvokeOnUI(() =>
						{
							if (_selectMonitor.Busy)
								return;

							using (_selectMonitor.Enter())
							{
								WordListsGrid.SelectedCellRanges.Clear();
								if (vm.SelectedVarietyMeaning != null)
								{
									WordListsVarietyViewModel variety = vm.SelectedVarietyMeaning.Variety;
									int itemIndex = WordListsGrid.Items.IndexOf(variety);
									WordListsGrid.BringItemIntoView(variety);
									WordListsGrid.Dispatcher.BeginInvoke(new Action(() =>
									    {
									        var row = (DataRow) WordListsGrid.GetContainerFromIndex(itemIndex);
										    if (row != null)
										    {
											    Cell cell = row.Cells.Single(c => c.Content == vm.SelectedVarietyMeaning);
												WordListsGrid.SelectedCellRanges.Add(new SelectionCellRange(itemIndex, cell.ParentColumn.Index));
												WordListsGrid.CurrentItem = variety;
												WordListsGrid.CurrentColumn = cell.ParentColumn;
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
			if (vm == null)
				return;

			WordListsGrid.CurrentColumn = null;
			WordListsGrid.CurrentItem = null;
			WordListsGrid.Columns.Clear();
			var view = new DataGridCollectionView(vm.Varieties, typeof(WordListsVarietyViewModel), false, false);
			view.ItemProperties.Add(new DataGridItemProperty("Variety", ".", typeof(WordListsVarietyViewModel)));
			IComparer sortComparer = ProjectionComparer<WordListsVarietyMeaningViewModel>.Create(meaning => meaning.StrRep);
			for (int i = 0; i < vm.Meanings.Count; i++)
				view.ItemProperties.Add(new DataGridItemProperty("Meaning" + i, $"Meanings[{i}]", typeof(WordListsVarietyMeaningViewModel)) {SortComparer = sortComparer});
			vm.VarietiesView = view;
			WordListsGrid.Items.SortDescriptions.Clear();

			var headerColumn = new Column {FieldName = "Variety"};
			DataGridControlBehaviors.SetIsRowHeader(headerColumn, true);
			DataGridControlBehaviors.SetAutoSize(headerColumn, true);
			DataGridControlBehaviors.SetAutoSizePadding(headerColumn, 18);
			WordListsGrid.Columns.Add(headerColumn);
			for (int i = 0; i < vm.Meanings.Count; i++)
			{
				var column = new Column {FieldName = "Meaning" + i, Width = 100, CellEditor = WordListsGrid.DefaultCellEditors[typeof(WordListsVarietyMeaningViewModel)]};
				var titleBinding = new Binding($"DataGridControl.DataContext.Meanings[{i}].Gloss") {RelativeSource = RelativeSource.Self};
				BindingOperations.SetBinding(column, ColumnBase.TitleProperty, titleBinding);
				WordListsGrid.Columns.Add(column);
			}
		}

		private void Meanings_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			LoadCollectionView();
		}

		private void WordListsGrid_OnSelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
		{
			if (_selectMonitor.Busy)
				return;

			using (_selectMonitor.Enter())
			{
				var vm = (WordListsViewModel) DataContext;
				if (WordListsGrid.SelectedCellRanges.Count == 1)
				{
					SelectionCellRange cellRange = WordListsGrid.SelectedCellRanges[0];
					int itemIndex = cellRange.ItemRange.StartIndex;
					var variety = (WordListsVarietyViewModel) WordListsGrid.Items[itemIndex];
					int columnIndex = cellRange.ColumnRange.StartIndex;
					vm.SelectedVarietyMeaning = variety.Meanings[columnIndex - 1];
				}
				else
				{
					vm.SelectedVarietyMeaning = null;
				}
			}
		}

		private void Cell_OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			var cell = (DataCell) sender;
			if (cell.ParentColumn.Index != 0)
				cell.Focus();
		}

		private void Cell_OnPreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
		{
			var cell = (DataCell) sender;
			if (cell.ParentColumn.Index != 0)
				cell.Focus();
		}

		private void WordListsGrid_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			Cell activeCell = ActiveCell;
			if (activeCell == null)
				return;
			ColumnBase activeColumn = WordListsGrid.CurrentColumn;
			object activeRow = WordListsGrid.CurrentItem;

			switch (e.Key)
			{
				//These first 4 cases prevent the user from navigating off the edges of the grid
				case Key.Down:
					if (WordListsGrid.Items.IndexOf(activeRow) == WordListsGrid.Items.Count - 1)
						e.Handled = true;
					break;
				case Key.Up:
					if (WordListsGrid.Items.IndexOf(activeRow) == 0)
						e.Handled = true;
					break;
				case Key.Left:
					if (activeColumn.Index == WordListsGrid.VisibleColumns[0].Index)
						e.Handled = true;
					break;
				case Key.Right:
					if (activeColumn.Index == WordListsGrid.VisibleColumns[WordListsGrid.VisibleColumns.Count - 1].Index)
						e.Handled = true;
					break;

				//Tab key should act as though the user hit 'Right'
				case Key.Tab:
					if (activeCell.IsBeingEdited) activeCell.EndEdit();
					if (activeColumn.Index != WordListsGrid.VisibleColumns[WordListsGrid.VisibleColumns.Count - 1].Index)
						WordListsGrid.CurrentColumn = WordListsGrid.VisibleColumns[WordListsGrid.VisibleColumns.IndexOf(activeColumn) + 1];
					SelectActiveCell();
					e.Handled = true;
					break;

				//Enter key should act as though the user hit 'Down'
				case Key.Enter:
					if (activeCell.IsBeingEdited) activeCell.EndEdit();
					if (WordListsGrid.Items.IndexOf(activeRow) != WordListsGrid.Items.Count - 1)
						WordListsGrid.CurrentItem = WordListsGrid.Items[WordListsGrid.Items.IndexOf(activeRow) + 1];
					SelectActiveCell();
					e.Handled = true;
					break;
			}
		}
	}
}
