using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using GalaSoft.MvvmLight.Threading;
using SIL.Cog.Applications.ViewModels;
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
			BusyCursor.DisplayUntilIdle();
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			var vm = (MultipleWordAlignmentViewModel) DataContext;
			vm.SensesView = CollectionViewSource.GetDefaultView(vm.Senses);
			LoadCollectionView();
			if (vm.GroupByCognateSet)
				vm.WordsView.GroupDescriptions.Add(new DataGridGroupDescription("CognateSetIndex"));
			vm.Words.CollectionChanged += WordsChanged;
			vm.PropertyChanged += vm_PropertyChanged;
		}

		private void vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var vm = (MultipleWordAlignmentViewModel) DataContext;
			switch (e.PropertyName)
			{
				case "Senses":
					DispatcherHelper.CheckBeginInvokeOnUI(() => vm.SensesView = CollectionViewSource.GetDefaultView(vm.Senses));
					break;

				case "GroupByCognateSet":
					if (vm.GroupByCognateSet)
						vm.WordsView.GroupDescriptions.Add(new PropertyGroupDescription("CognateSetIndex"));
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

			var view = new DataGridCollectionView(vm.Words, typeof(MultipleWordAlignmentWordViewModel), false, false);
			view.ItemProperties.Add(new DataGridItemProperty("Variety", "Variety.Name", typeof(string)));
			view.ItemProperties.Add(new DataGridItemProperty("StrRep", "StrRep", typeof(string)));
			view.ItemProperties.Add(new DataGridItemProperty("CognateSetIndex", "CognateSetIndex", typeof(int)));
			view.ItemProperties.Add(new DataGridItemProperty("Prefix", "Prefix", typeof(string)));
			for (int i = 0; i < vm.ColumnCount; i++)
				view.ItemProperties.Add(new DataGridItemProperty("Column" + i, string.Format("Columns[{0}]", i), typeof(string)));
			view.ItemProperties.Add(new DataGridItemProperty("Suffix", "Suffix", typeof(string)));
			vm.WordsView = view;

			AlignmentGrid.Columns.Clear();
			var headerColumn = new Column {FieldName = "Variety"};
			DataGridControlBehaviors.SetIsRowHeader(headerColumn, true);
			AlignmentGrid.Columns.Add(headerColumn);
			headerColumn.SetWidthToFit<MultipleWordAlignmentWordViewModel>(w => w.Variety.Name, 18);
			var prefixColumn = new Column {FieldName = "Prefix", ReadOnly = true, CanBeCurrentWhenReadOnly = false};
			AlignmentGrid.Columns.Add(prefixColumn);
			prefixColumn.SetWidthToFit<MultipleWordAlignmentWordViewModel>(w => w.Prefix, 9, 16);
			for (int i = 0; i < vm.ColumnCount; i++)
			{
				var column = new Column {FieldName = "Column" + i};
				AlignmentGrid.Columns.Add(column);
				column.SetWidthToFit<MultipleWordAlignmentWordViewModel>(w => w.Columns[i], 9, 16);
			}
			var suffixColumn = new Column {FieldName = "Suffix", ReadOnly = true, CanBeCurrentWhenReadOnly = false};
			AlignmentGrid.Columns.Add(suffixColumn);
			suffixColumn.SetWidthToFit<MultipleWordAlignmentWordViewModel>(w => w.Suffix, 9, 16);

			AlignmentGrid.CurrentItem = null;
		}

		private void AlignmentGrid_OnSelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
		{
			var vm = (MultipleWordAlignmentViewModel) DataContext;
			if (e.SelectionInfos.Count == 1 && e.SelectionInfos[0].AddedCellRanges.Count == 1)
			{
				SelectionCellRange range = e.SelectionInfos[0].AddedCellRanges[0];
				vm.CurrentColumn = range.ColumnRange.StartIndex - 2;
				vm.CurrentWord = (MultipleWordAlignmentWordViewModel) AlignmentGrid.Items[range.ItemRange.StartIndex];
			}
			else
			{
				vm.CurrentColumn = -1;
				vm.CurrentWord = null;
				AlignmentGrid.CurrentItem = null;
			}
		}


	}
}
