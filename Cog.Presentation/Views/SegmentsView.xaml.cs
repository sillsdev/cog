using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Linq;
using GalaSoft.MvvmLight.Threading;
using SIL.Cog.Applications.ViewModels;
using SIL.Cog.Presentation.Controls;
using Xceed.Wpf.DataGrid;
using Xceed.Wpf.DataGrid.Views;

namespace SIL.Cog.Presentation.Views
{
	/// <summary>
	/// Interaction logic for SegmentsView.xaml
	/// </summary>
	public partial class SegmentsView
	{
		public SegmentsView()
		{
			InitializeComponent();
			BusyCursor.DisplayUntilIdle();
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			LoadColumns();
			LoadCollectionView();
			LoadMergedHeaders();
			SizeRowSelectorPaneToFit();
			SelectFirstCell();
		}

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var vm = DataContext as SegmentsViewModel;
			if (vm == null)
				return;

			vm.PropertyChanged += ViewModel_PropertyChanged;
			vm.Segments.CollectionChanged += Segments_CollectionChanged;
			vm.Varieties.CollectionChanged += Varieties_CollectionChanged;
			vm.Categories.CollectionChanged += Categories_CollectionChanged;
		}

		private void Categories_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			LoadMergedHeaders();
		}

		private void Segments_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			LoadColumns();
			LoadCollectionView();
		}

		private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var vm = (SegmentsViewModel) sender;
			switch (e.PropertyName)
			{
				case "Varieties":
					DispatcherHelper.CheckBeginInvokeOnUI(() =>
						{
							LoadCollectionView();
							SizeRowSelectorPaneToFit();
							vm.Varieties.CollectionChanged += Varieties_CollectionChanged;
						});
					SegmentsDataGrid.Dispatcher.BeginInvoke(new Action(SelectFirstCell));
					break;
			}
		}

		private void Varieties_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			SegmentsDataGrid.Items.Refresh();
			SizeRowSelectorPaneToFit();
		}

		private void LoadColumns()
		{
			var vm = (SegmentsViewModel) DataContext;
			SegmentsDataGrid.Columns.Clear();
			for (int i = 0; i < vm.Segments.Count; i++)
			{
				var c = new Column
					{
						FieldName = vm.Segments[i].StrRep,
						Title = vm.Segments[i].StrRep,
						DisplayMemberBindingInfo = new DataGridBindingInfo { Path = new PropertyPath(string.Format("Segments[{0}].Frequency", i)), ReadOnly = true},
						Width = new ColumnWidth(60),
						CellHorizontalContentAlignment = HorizontalAlignment.Center
					};
				SegmentsDataGrid.Columns.Add(c);
			}
		}

		private void LoadCollectionView()
		{
			var vm = (SegmentsViewModel) DataContext;
			if (vm == null)
				return;

			var source = new DataGridCollectionView(vm.Varieties, typeof(SegmentsVarietyViewModel), false, false);
			for (int i = 0; i < vm.Segments.Count; i++)
				source.ItemProperties.Add(new DataGridItemProperty(vm.Segments[i].StrRep, string.Format("Segments[{0}].Frequency", i), typeof(string)));
			SegmentsDataGrid.ItemsSource = source;
		}

		private void LoadMergedHeaders()
		{
			ObservableCollection<MergedHeader> mergedHeaders = MergedHeadersPanel.GetMergedHeaders(SegmentsDataGrid);
			var vm = (SegmentsViewModel) DataContext;
			mergedHeaders.Clear();
			foreach (SegmentCategoryViewModel category in vm.Categories)
			{
			    var header = new MergedHeader {Title = category.Name};
			    header.ColumnNames.AddRange(category.Segments.Select(s => s.StrRep));
			    mergedHeaders.Add(header);
			}
		}

		private void SizeRowSelectorPaneToFit()
		{
			var vm = (SegmentsViewModel) DataContext;
			if (vm == null)
				return;

			var textBrush = (Brush) Application.Current.FindResource("HeaderTextBrush");
			double maxWidth = 0;
			foreach (SegmentsVarietyViewModel variety in vm.Varieties)
			{
				var formattedText = new FormattedText(variety.Name, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
					new Typeface(SegmentsDataGrid.FontFamily, SegmentsDataGrid.FontStyle, SegmentsDataGrid.FontWeight, SegmentsDataGrid.FontStretch), SegmentsDataGrid.FontSize, textBrush);
				if (formattedText.Width > maxWidth)
					maxWidth = formattedText.Width;
				variety.PropertyChanged -= variety_PropertyChanged;
				variety.PropertyChanged += variety_PropertyChanged;
			}

			var tableView = (TableView) SegmentsDataGrid.View;
			tableView.RowSelectorPaneWidth = maxWidth + 18;
		}

		private void variety_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "Name":
					DispatcherHelper.CheckBeginInvokeOnUI(SizeRowSelectorPaneToFit);
					break;
			}
		}

		private void SelectFirstCell()
		{
			if (SegmentsDataGrid.Items.Count > 0)
				SegmentsDataGrid.SelectedCellRanges.Add(new SelectionCellRange(0, 0));
			SegmentsDataGrid.Focus();
		}
	}
}
