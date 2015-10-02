using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
			BusyCursor.DisplayUntilIdle();
		}

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var vm = DataContext as WordListsViewModel;
			if (vm == null)
				return;

			vm.PropertyChanged += ViewModel_PropertyChanged;
			vm.Meanings.CollectionChanged += Meanings_CollectionChanged;
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

			WordListsGrid.CurrentColumn = null;
			WordListsGrid.CurrentItem = null;
			WordListsGrid.Columns.Clear();
			var view = new DataGridCollectionView(vm.Varieties, typeof(WordListsVarietyViewModel), false, false);
			view.ItemProperties.Add(new DataGridItemProperty("Variety", ".", typeof(WordListsVarietyViewModel)));
			IComparer sortComparer = ProjectionComparer<WordListsVarietyMeaningViewModel>.Create(meaning => meaning.StrRep);
			for (int i = 0; i < vm.Meanings.Count; i++)
				view.ItemProperties.Add(new DataGridItemProperty("Meaning" + i, string.Format("Meanings[{0}]", i), typeof(WordListsVarietyMeaningViewModel)) {SortComparer = sortComparer});
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
				var titleBinding = new Binding(string.Format("DataGridControl.DataContext.Meanings[{0}].Gloss", i)) {RelativeSource = RelativeSource.Self};
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

		private void QuickReference_Click(object sender, RoutedEventArgs e)
		{
			string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			if (!string.IsNullOrEmpty(exeDir))
				Process.Start(Path.Combine(exeDir, "Help\\GettingStartedWithCog.pdf"));
		}
	}
}
