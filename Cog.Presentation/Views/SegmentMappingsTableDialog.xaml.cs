using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Threading;
using SIL.Cog.Application.ViewModels;
using SIL.Cog.Presentation.Behaviors;
using SIL.Collections;
using Xceed.Wpf.DataGrid;

namespace SIL.Cog.Presentation.Views
{
	public partial class SegmentMappingsTableDialog
	{
		private readonly SimpleMonitor _selectMonitor;

		public SegmentMappingsTableDialog()
		{
			_selectMonitor = new SimpleMonitor();
			InitializeComponent();
			SegmentsDataGrid.ClipboardExporters.Clear();
			BusyCursor.DisplayUntilIdle();
		}

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var vm = DataContext as SegmentMappingsTableViewModel;
			if (vm == null)
				return;

			vm.PropertyChanged += ViewModel_PropertyChanged;
		}

		private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var vm = (SegmentMappingsTableViewModel) sender;
			switch (e.PropertyName)
			{
				case "SelectedSegmentPair":
					DispatcherHelper.CheckBeginInvokeOnUI(() =>
					{
						if (_selectMonitor.Busy)
							return;

						using (_selectMonitor.Enter())
						{
							SegmentsDataGrid.SelectedCellRanges.Clear();
							if (vm.SelectedSegmentPair != null)
							{
								SegmentMappingsTableSegmentViewModel segment = vm.SelectedSegmentPair.Segment1;
								int itemIndex = SegmentsDataGrid.Items.IndexOf(segment);
								SegmentsDataGrid.BringItemIntoView(segment);
								SegmentsDataGrid.Dispatcher.BeginInvoke(new Action(() =>
								{
									var row = (DataRow) SegmentsDataGrid.GetContainerFromIndex(itemIndex);
									if (row != null)
									{
										Cell cell = row.Cells.Single(c => c.Content == vm.SelectedSegmentPair);
										SegmentsDataGrid.SelectedCellRanges.Add(new SelectionCellRange(itemIndex, cell.ParentColumn.Index));
										SegmentsDataGrid.CurrentItem = segment;
										SegmentsDataGrid.CurrentColumn = cell.ParentColumn;
										cell.BringIntoView();
									}
								}), DispatcherPriority.Background);
							}
						}
					});
					break;
			}
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			LoadCollectionView();
			LoadMergedHeaders();
		}

		private void LoadCollectionView()
		{
			var vm = (SegmentMappingsTableViewModel) DataContext;

			SegmentsDataGrid.CurrentColumn = null;
			SegmentsDataGrid.Columns.Clear();
			var view = new DataGridCollectionView(vm.Segments.Reverse(), typeof (SegmentMappingsTableSegmentViewModel), false, false);
			view.ItemProperties.Add(new DataGridItemProperty("Segment", ".", typeof(SegmentMappingsTableSegmentViewModel)));
			for (int i = 0; i < vm.Segments.Count; i++)
				view.ItemProperties.Add(new DataGridItemProperty(vm.Segments[i].StrRep, string.Format("SegmentPairs[{0}]", i), typeof(SegmentMappingsTableSegmentPairViewModel)));
			SegmentsDataGrid.ItemsSource = view;

			var headerColumn = new Column {FieldName = "Segment"};
			DataGridControlBehaviors.SetIsRowHeader(headerColumn, true);
			headerColumn.Width = 30;
			headerColumn.CellVerticalContentAlignment = VerticalAlignment.Center;
			headerColumn.CellHorizontalContentAlignment = HorizontalAlignment.Center;
			SegmentsDataGrid.Columns.Add(headerColumn);
			foreach (SegmentMappingsTableSegmentViewModel segment in vm.Segments)
			{
				SegmentsDataGrid.Columns.Add(new Column
				{
					FieldName = segment.StrRep,
					Title = segment.StrRep,
					Width = 30,
					AllowSort = false,
					CellHorizontalContentAlignment = HorizontalAlignment.Center,
					CellVerticalContentAlignment = VerticalAlignment.Center,
					CellContentTemplate = (DataTemplate) SegmentsDataGrid.Resources["SegmentPairTemplate"],
					CellEditor = SegmentsDataGrid.DefaultCellEditors[typeof(SegmentMappingsTableSegmentPairViewModel)]
				});
			}
		}

		private void LoadMergedHeaders()
		{
			ObservableCollection<MergedHeader> mergedHeaders = DataGridControlBehaviors.GetMergedHeaders(SegmentsDataGrid);
			var vm = (SegmentMappingsTableViewModel) DataContext;
			mergedHeaders.Clear();
			if (vm.Categories.Count > 0)
			{
				mergedHeaders.Add(new MergedHeader {ColumnNames = {"Segment"}});
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
			if (_selectMonitor.Busy)
				return;

			using (_selectMonitor.Enter())
			{
				var vm = (SegmentMappingsTableViewModel) DataContext;
				if (SegmentsDataGrid.SelectedCellRanges.Count == 1)
				{
					SelectionCellRange cellRange = SegmentsDataGrid.SelectedCellRanges[0];
					int itemIndex = cellRange.ItemRange.StartIndex;
					var segment = (SegmentMappingsTableSegmentViewModel) SegmentsDataGrid.Items[itemIndex];
					int columnIndex = cellRange.ColumnRange.StartIndex;
					SegmentMappingsTableSegmentPairViewModel segmentPair = segment.SegmentPairs[columnIndex - 1];
					vm.SelectedSegmentPair = segmentPair.IsEnabled ? segmentPair : null;
				}
				else
				{
					vm.SelectedSegmentPair = null;
				}
			}
		}
	}
}
