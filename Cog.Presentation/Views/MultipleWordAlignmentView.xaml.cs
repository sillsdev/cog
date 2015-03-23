using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
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
	/// Interaction logic for MultipleWordAlignmentView.xaml
	/// </summary>
	public partial class MultipleWordAlignmentView
	{
		private readonly SimpleMonitor _monitor;

		public MultipleWordAlignmentView()
		{
			InitializeComponent();
			AlignmentGrid.ClipboardExporters.Clear();
			BusyCursor.DisplayUntilIdle();
			_monitor = new SimpleMonitor();
		}

		private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (IsVisible)
				Dispatcher.BeginInvoke(new Action(() => MeaningsComboBox.Focus()));
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			var vm = (MultipleWordAlignmentViewModel) DataContext;
			vm.MeaningsView = CollectionViewSource.GetDefaultView(vm.Meanings);
			MultipleWordAlignmentWordViewModel[] selectedWords = vm.SelectedWords.ToArray();
			LoadCollectionView();
			SelectWords(selectedWords);
			vm.Words.CollectionChanged += WordsChanged;
			vm.SelectedWords.CollectionChanged += SelectedWordsChanged;
			vm.PropertyChanged += vm_PropertyChanged;
		}

		private void vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var vm = (MultipleWordAlignmentViewModel) sender;
			switch (e.PropertyName)
			{
				case "Meanings":
					DispatcherHelper.CheckBeginInvokeOnUI(() => vm.MeaningsView = CollectionViewSource.GetDefaultView(vm.Meanings));
					break;

				case "GroupByCognateSet":
					if (vm.GroupByCognateSet)
						vm.WordsView.GroupDescriptions.Add(new DataGridGroupDescription("CognateSetIndex"));
					else
						vm.WordsView.GroupDescriptions.Clear();
					break;
			}
		}

		private void WordsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			LoadCollectionView();
		}

		private void SelectedWordsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (_monitor.Busy)
				return;
			
			using (_monitor.Enter())
				SelectWords((IEnumerable<MultipleWordAlignmentWordViewModel>) sender);
		}

		private void SelectWords(IEnumerable<MultipleWordAlignmentWordViewModel> selectedWords)
		{
			AlignmentGrid.SelectedItems.Clear();
			foreach (MultipleWordAlignmentWordViewModel word in selectedWords)
				AlignmentGrid.SelectedItems.Add(word);
			if (AlignmentGrid.SelectedItems.Count > 0)
			{
				AlignmentGrid.Dispatcher.BeginInvoke(new Action(() =>
				{
					int index = AlignmentGrid.SelectedItemRanges.Select(ir => ir.EndIndex).Max();
					AlignmentGrid.BringItemIntoView(AlignmentGrid.Items[index]);
					AlignmentGrid.Focus();
				}), DispatcherPriority.Background);
			}
		}

		private void LoadCollectionView()
		{
			var vm = (MultipleWordAlignmentViewModel) DataContext;

			AlignmentGrid.Columns.Clear();

			var view = new DataGridCollectionView(vm.Words, typeof(MultipleWordAlignmentWordViewModel), false, false);
			view.ItemProperties.Add(new DataGridItemProperty("Variety", "Variety", typeof(VarietyViewModel)) {SortComparer = ProjectionComparer<VarietyViewModel>.Create(v => v.Name)});
			view.ItemProperties.Add(new DataGridItemProperty("StrRep", "StrRep", typeof(string)));
			view.ItemProperties.Add(new DataGridItemProperty("CognateSetIndex", "CognateSetIndex", typeof(int)));
			view.ItemProperties.Add(new DataGridItemProperty("Prefix", "Prefix", typeof(string)));
			for (int i = 0; i < vm.ColumnCount; i++)
				view.ItemProperties.Add(new DataGridItemProperty("Column" + i, string.Format("Columns[{0}]", i), typeof(string)));
			view.ItemProperties.Add(new DataGridItemProperty("Suffix", "Suffix", typeof(string)));
			if (vm.GroupByCognateSet)
			{
				Debug.Assert(view.GroupDescriptions != null);
				view.GroupDescriptions.Add(new DataGridGroupDescription("CognateSetIndex"));
			}
			vm.WordsView = view;

			var headerColumn = new Column {FieldName = "Variety"};
			DataGridControlBehaviors.SetIsRowHeader(headerColumn, true);
			DataGridControlBehaviors.SetAutoSize(headerColumn, true);
			DataGridControlBehaviors.SetAutoSizePadding(headerColumn, 18);
			AlignmentGrid.Columns.Add(headerColumn);

			object fontSizeObj = System.Windows.Application.Current.FindResource("PhoneticFontSize");
			Debug.Assert(fontSizeObj != null);
			var fontSize = (double) fontSizeObj;

			var prefixColumn = new Column {FieldName = "Prefix", ReadOnly = true, CanBeCurrentWhenReadOnly = false};
			DataGridControlBehaviors.SetAutoSize(prefixColumn, true);
			DataGridControlBehaviors.SetAutoSizePadding(prefixColumn, 9);
			DataGridControlBehaviors.SetFontSizeHint(prefixColumn, fontSize);
			AlignmentGrid.Columns.Add(prefixColumn);
			for (int i = 0; i < vm.ColumnCount; i++)
			{
				var column = new Column {FieldName = "Column" + i};
				DataGridControlBehaviors.SetAutoSize(column, true);
				DataGridControlBehaviors.SetAutoSizePadding(column, 9);
				DataGridControlBehaviors.SetFontSizeHint(column, fontSize);
				AlignmentGrid.Columns.Add(column);
			}
			var suffixColumn = new Column {FieldName = "Suffix", ReadOnly = true, CanBeCurrentWhenReadOnly = false};
			DataGridControlBehaviors.SetAutoSize(suffixColumn, true);
			DataGridControlBehaviors.SetAutoSizePadding(suffixColumn, 9);
			DataGridControlBehaviors.SetFontSizeHint(suffixColumn, fontSize);
			AlignmentGrid.Columns.Add(suffixColumn);

			AlignmentGrid.CurrentItem = null;
		}

		private void AlignmentGrid_OnSelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
		{
			if (_monitor.Busy)
				return;

			using (_monitor.Enter())
			{
				var vm = (MultipleWordAlignmentViewModel) DataContext;
				vm.SelectedWords.Clear();
				vm.SelectedWords.AddRange(AlignmentGrid.SelectedItems.Cast<MultipleWordAlignmentWordViewModel>());
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
	}
}
