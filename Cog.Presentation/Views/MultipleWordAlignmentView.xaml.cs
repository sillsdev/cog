using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using GalaSoft.MvvmLight.Threading;
using SIL.Cog.Application.ViewModels;
using SIL.Cog.Presentation.Behaviors;
using Xceed.Wpf.DataGrid;

namespace SIL.Cog.Presentation.Views
{
	/// <summary>
	/// Interaction logic for MultipleWordAlignmentView.xaml
	/// </summary>
	public partial class MultipleWordAlignmentView
	{
		public MultipleWordAlignmentView()
		{
			InitializeComponent();
			AlignmentGrid.ClipboardExporters.Clear();
			BusyCursor.DisplayUntilIdle();
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
			LoadCollectionView();
			vm.Words.CollectionChanged += WordsChanged;
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

		private void LoadCollectionView()
		{
			var vm = (MultipleWordAlignmentViewModel) DataContext;

			AlignmentGrid.Columns.Clear();

			var view = new DataGridCollectionView(vm.Words, typeof(MultipleWordAlignmentWordViewModel), false, false);
			view.ItemProperties.Add(new DataGridItemProperty("Variety", "Variety", typeof(VarietyViewModel)));
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
			var vm = (MultipleWordAlignmentViewModel) DataContext;
			if (e.SelectionInfos.Count == 1 && e.SelectionInfos[0].AddedCellRanges.Count == 1)
			{
				SelectionCellRange range = e.SelectionInfos[0].AddedCellRanges[0];
				vm.SelectedColumn = range.ColumnRange.StartIndex - 2;
				vm.SelectedWord = (MultipleWordAlignmentWordViewModel) AlignmentGrid.Items[range.ItemRange.StartIndex];
			}
			else
			{
				vm.SelectedColumn = -1;
				vm.SelectedWord = null;
				AlignmentGrid.CurrentItem = null;
			}
		}
	}
}
