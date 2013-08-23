using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Linq;
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
	/// Interaction logic for SegmentsView.xaml
	/// </summary>
	public partial class SegmentsView
	{
		private readonly SimpleMonitor _selectMonitor;
		private InputBinding _findBinding;

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
			var window = this.FindVisualAncestor<Window>();
			if (IsVisible)
			{
				window.InputBindings.Add(_findBinding);
				Dispatcher.BeginInvoke(new Action(() => SegmentsDataGrid.Focus()));
			}
			else
			{
				window.InputBindings.Remove(_findBinding);
			}
		}

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var vm = DataContext as SegmentsViewModel;
			if (vm == null)
				return;

			vm.PropertyChanged += ViewModel_PropertyChanged;
			vm.Segments.CollectionChanged += Segments_CollectionChanged;
			vm.Varieties.CollectionChanged += Varieties_CollectionChanged;
			AddVarieties(vm.Varieties);
			vm.Categories.CollectionChanged += Categories_CollectionChanged;
			_findBinding = new InputBinding(vm.FindCommand, new KeyGesture(Key.F, ModifierKeys.Control));
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
					vm.Varieties.CollectionChanged += Varieties_CollectionChanged;
					AddVarieties(vm.Varieties);
					DispatcherHelper.CheckBeginInvokeOnUI(LoadCollectionView);
					break;

				case "CurrentSegment":
					DispatcherHelper.CheckBeginInvokeOnUI(() =>
						{
							if (_selectMonitor.Busy)
								return;

							using (_selectMonitor.Enter())
							{
								SegmentsDataGrid.SelectedCellRanges.Clear();
								if (vm.CurrentSegment != null)
								{
									VarietyViewModel variety = vm.CurrentSegment.Variety;
									int itemIndex = SegmentsDataGrid.Items.IndexOf(variety);
									SegmentsDataGrid.BringItemIntoView(variety);
									SegmentsDataGrid.Dispatcher.BeginInvoke(new Action(() =>
									    {
									        var row = (DataRow) SegmentsDataGrid.GetContainerFromIndex(itemIndex);
										    if (row != null)
										    {
											    Cell cell = row.Cells.Single(c => c.DataContext == vm.CurrentSegment);
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

		private void Varieties_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					AddVarieties(e.NewItems.Cast<SegmentsVarietyViewModel>());
					break;

				case NotifyCollectionChangedAction.Remove:
					RemoveVarieties(e.OldItems.Cast<SegmentsVarietyViewModel>());
					break;

				case NotifyCollectionChangedAction.Replace:
					RemoveVarieties(e.OldItems.Cast<SegmentsVarietyViewModel>());
					AddVarieties(e.NewItems.Cast<SegmentsVarietyViewModel>());
					break;

				case NotifyCollectionChangedAction.Reset:
					AddVarieties(((IEnumerable) sender).Cast<SegmentsVarietyViewModel>());
					break;
			}

			Dispatcher.BeginInvoke(new Action(() => SegmentsDataGrid.Columns[0].SetWidthToFit<SegmentsVarietyViewModel>(v => v.Name, 18)));
		}

		private void AddVarieties(IEnumerable<SegmentsVarietyViewModel> varieties)
		{
			foreach (SegmentsVarietyViewModel variety in varieties)
				variety.PropertyChanged += variety_PropertyChanged;
		}

		private void RemoveVarieties(IEnumerable<SegmentsVarietyViewModel> varieties)
		{
			foreach (SegmentsVarietyViewModel variety in varieties)
				variety.PropertyChanged -= variety_PropertyChanged;
		}

		private void LoadCollectionView()
		{
			var vm = (SegmentsViewModel) DataContext;
			var view = new DataGridCollectionView(vm.Varieties, typeof(SegmentsVarietyViewModel), false, false);
			view.ItemProperties.Add(new DataGridItemProperty("Variety", "Name", typeof(string)));
			for (int i = 0; i < vm.Segments.Count; i++)
				view.ItemProperties.Add(new DataGridItemProperty(vm.Segments[i].StrRep, string.Format("Segments[{0}].Frequency", i), typeof(string)));
			SegmentsDataGrid.ItemsSource = view;
			SegmentsDataGrid.Items.SortDescriptions.Clear();

			SegmentsDataGrid.Columns.Clear();
			var headerColumn = new Column {FieldName = "Variety"};
			DataGridControlBehaviors.SetIsRowHeader(headerColumn, true);
			SegmentsDataGrid.Columns.Add(headerColumn);
			headerColumn.SetWidthToFit<SegmentsVarietyViewModel>(v => v.Name, 18);
			foreach (SegmentViewModel segment in vm.Segments)
				SegmentsDataGrid.Columns.Add(new Column {FieldName = segment.StrRep, Title = segment.StrRep, Width = 60, CellHorizontalContentAlignment = HorizontalAlignment.Center});
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

		private void variety_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "Name":
					DispatcherHelper.CheckBeginInvokeOnUI(() => SegmentsDataGrid.Columns[0].SetWidthToFit<SegmentsVarietyViewModel>(v => v.Name, 18));
					break;
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
					vm.CurrentSegment = variety.Segments[columnIndex - 1];
				}
				else
				{
					vm.CurrentSegment = null;
				}
			}
		}
	}
}
