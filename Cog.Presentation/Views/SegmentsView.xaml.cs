using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Linq;
using GalaSoft.MvvmLight.Threading;
using SIL.Cog.Applications.ViewModels;
using SIL.Cog.Presentation.Behaviors;
using Xceed.Wpf.DataGrid;

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
			LoadCollectionView();
			LoadMergedHeaders();
			SegmentsDataGrid.SelectFirstCell();
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
			vm.Varieties.CollectionChanged += Varieties_CollectionChanged;
			AddVarieties(vm.Varieties);
			vm.Categories.CollectionChanged += Categories_CollectionChanged;
		}

		private void Categories_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			LoadMergedHeaders();
		}

		private void Segments_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			LoadCollectionView();
			SegmentsDataGrid.SelectFirstCell();
		}

		private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var vm = (SegmentsViewModel) sender;
			switch (e.PropertyName)
			{
				case "Varieties":
					vm.Varieties.CollectionChanged += Varieties_CollectionChanged;
					AddVarieties(vm.Varieties);
					DispatcherHelper.CheckBeginInvokeOnUI(() =>
						{
							LoadCollectionView();
							SegmentsDataGrid.SelectFirstCell();
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
	}
}
