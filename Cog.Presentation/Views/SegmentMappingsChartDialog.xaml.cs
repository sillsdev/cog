using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using SIL.Cog.Application.ViewModels;
using SIL.Cog.Presentation.Behaviors;
using Xceed.Wpf.DataGrid;

namespace SIL.Cog.Presentation.Views
{
	public partial class SegmentMappingsChartDialog
	{
		public SegmentMappingsChartDialog()
		{
			InitializeComponent();
			SegmentsDataGrid.ClipboardExporters.Clear();
			BusyCursor.DisplayUntilIdle();
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			LoadCollectionView();
			LoadMergedHeaders();
		}

		private void okButton_Click(object sender, RoutedEventArgs e)
		{
			if (this.Validate())
				DialogResult = true;
		}

		private void LoadCollectionView()
		{
			var vm = (SegmentMappingsChartViewModel) DataContext;

			SegmentsDataGrid.CurrentColumn = null;
			SegmentsDataGrid.Columns.Clear();
			var view = new DataGridCollectionView(vm.Segments.Reverse(), typeof (SegmentMappingsChartSegmentViewModel), false, false);
			view.ItemProperties.Add(new DataGridItemProperty("Segment", ".", typeof(SegmentMappingsChartSegmentViewModel)));
			for (int i = 0; i < vm.Segments.Count; i++)
				view.ItemProperties.Add(new DataGridItemProperty(vm.Segments[i].StrRep, string.Format("SegmentPairs[{0}]", i), typeof(SegmentMappingsChartSegmentPairViewModel)));
			SegmentsDataGrid.ItemsSource = view;

			var headerColumn = new Column {FieldName = "Segment"};
			DataGridControlBehaviors.SetIsRowHeader(headerColumn, true);
			headerColumn.Width = 30;
			headerColumn.CellVerticalContentAlignment = VerticalAlignment.Center;
			headerColumn.CellHorizontalContentAlignment = HorizontalAlignment.Center;
			SegmentsDataGrid.Columns.Add(headerColumn);
			foreach (SegmentMappingsChartSegmentViewModel segment in vm.Segments)
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
					CellEditor = SegmentsDataGrid.DefaultCellEditors[typeof(SegmentMappingsChartSegmentPairViewModel)]
				});
			}
		}

		private void LoadMergedHeaders()
		{
			ObservableCollection<MergedHeader> mergedHeaders = DataGridControlBehaviors.GetMergedHeaders(SegmentsDataGrid);
			var vm = (SegmentMappingsChartViewModel) DataContext;
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
	}
}
