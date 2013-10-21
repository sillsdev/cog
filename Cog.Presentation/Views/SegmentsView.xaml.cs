using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Linq;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Threading;
using SIL.Cog.Applications.ViewModels;
using SIL.Cog.Presentation.Behaviors;
using SIL.Collections;
using Xceed.Wpf.DataGrid;

namespace SIL.Cog.Presentation.Views
{
	/// <summary>
	/// Interaction logic for SegmentsView.xaml
	/// </summary>
	public partial class SegmentsView
	{
		private readonly SimpleMonitor _selectMonitor;

		public SegmentsView()
		{
			InitializeComponent();
			SegmentsDataGrid.ClipboardExporters.Clear();
			_selectMonitor = new SimpleMonitor();
			BusyCursor.DisplayUntilIdle();
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			LoadCollectionView();
			LoadMergedHeaders();
		}

		private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (IsVisible)
				Dispatcher.BeginInvoke(new Action(() => SegmentsDataGrid.Focus()));
		}

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var vm = DataContext as SegmentsViewModel;
			if (vm == null)
				return;

			vm.PropertyChanged += ViewModel_PropertyChanged;
			vm.Segments.CollectionChanged += Segments_CollectionChanged;
			vm.Categories.CollectionChanged += Categories_CollectionChanged;
		}

		private void Categories_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			LoadMergedHeaders();
		}

		private void Segments_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			LoadCollectionView();
		}

		private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var vm = (SegmentsViewModel) sender;
			switch (e.PropertyName)
			{
				case "Varieties":
					DispatcherHelper.CheckBeginInvokeOnUI(LoadCollectionView);
					break;

				case "SelectedSegment":
					DispatcherHelper.CheckBeginInvokeOnUI(() =>
						{
							if (_selectMonitor.Busy)
								return;

							using (_selectMonitor.Enter())
							{
								SegmentsDataGrid.SelectedCellRanges.Clear();
								if (vm.SelectedSegment != null)
								{
									VarietyViewModel variety = vm.SelectedSegment.Variety;
									int itemIndex = SegmentsDataGrid.Items.IndexOf(variety);
									SegmentsDataGrid.BringItemIntoView(variety);
									SegmentsDataGrid.Dispatcher.BeginInvoke(new Action(() =>
									    {
									        var row = (DataRow) SegmentsDataGrid.GetContainerFromIndex(itemIndex);
										    if (row != null)
										    {
											    Cell cell = row.Cells.Single(c => c.DataContext == vm.SelectedSegment);
												SegmentsDataGrid.SelectedCellRanges.Add(new SelectionCellRange(itemIndex, cell.ParentColumn.Index));
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
			var vm = (SegmentsViewModel) DataContext;

			SegmentsDataGrid.Columns.Clear();
			var view = new DataGridCollectionView(vm.Varieties, typeof(SegmentsVarietyViewModel), false, false);
			view.ItemProperties.Add(new DataGridItemProperty("Variety", ".", typeof(SegmentsVarietyViewModel)));
			for (int i = 0; i < vm.Segments.Count; i++)
				view.ItemProperties.Add(new DataGridItemProperty(vm.Segments[i].StrRep, string.Format("Segments[{0}].Frequency", i), typeof(string)));
			SegmentsDataGrid.ItemsSource = view;
			SegmentsDataGrid.Items.SortDescriptions.Clear();

			var headerColumn = new Column {FieldName = "Variety"};
			DataGridControlBehaviors.SetIsRowHeader(headerColumn, true);
			DataGridControlBehaviors.SetAutoSize(headerColumn, true);
			SegmentsDataGrid.Columns.Add(headerColumn);
			foreach (SegmentViewModel segment in vm.Segments)
				SegmentsDataGrid.Columns.Add(new Column {FieldName = segment.StrRep, Title = segment.StrRep, Width = 63, CellHorizontalContentAlignment = HorizontalAlignment.Center});
		}

		private void LoadMergedHeaders()
		{
			ObservableCollection<MergedHeader> mergedHeaders = DataGridControlBehaviors.GetMergedHeaders(SegmentsDataGrid);
			var vm = (SegmentsViewModel) DataContext;
			mergedHeaders.Clear();
			if (vm.Categories.Count > 0)
			{
				mergedHeaders.Add(new MergedHeader {ColumnNames = {"Variety"}});
				foreach (SegmentCategoryViewModel category in vm.Categories)
				{
					var header = new MergedHeader {Title = category.Name};
					header.ColumnNames.AddRange(category.Segments.Select(s => s.StrRep));
					mergedHeaders.Add(header);
				}
			}
		}

		private void SegmentsDataGrid_OnSelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
		{
			var vm = (SegmentsViewModel) DataContext;
			if (_selectMonitor.Busy)
				return;

			using (_selectMonitor.Enter())
			{
				if (SegmentsDataGrid.SelectedCellRanges.Count == 1)
				{
					SelectionCellRange cellRange = SegmentsDataGrid.SelectedCellRanges[0];
					int itemIndex = cellRange.ItemRange.StartIndex;
					var variety = (SegmentsVarietyViewModel) SegmentsDataGrid.Items[itemIndex];
					int columnIndex = cellRange.ColumnRange.StartIndex;
					vm.SelectedSegment = variety.Segments[columnIndex - 1];
				}
				else
				{
					vm.SelectedSegment = null;
				}
			}
		}
	}
}
