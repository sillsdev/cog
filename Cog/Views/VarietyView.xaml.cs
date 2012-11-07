using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using SIL.Cog.ViewModels;

namespace SIL.Cog.Views
{
	/// <summary>
	/// Interaction logic for VarietyView.xaml
	/// </summary>
	public partial class VarietyView
	{
		public VarietyView()
		{
			InitializeComponent();
		}

		private void VarietyView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var vm = DataContext as VarietyVarietiesViewModel;
			if (vm == null)
				return;

			var wordsSource = new ListCollectionView(vm.Words);
			wordsSource.GroupDescriptions.Add(new PropertyGroupDescription("Sense"));
			_wordsControl.ItemsSource = wordsSource;
			wordsSource.SortDescriptions.Add(new SortDescription("Sense.Gloss", ListSortDirection.Ascending));
			wordsSource.Refresh();

			var segmentsSource = new ListCollectionView(vm.Segments);
			_segmentsDataGrid.ItemsSource = segmentsSource;
			segmentsSource.SortDescriptions.Add(new SortDescription("Probability", ListSortDirection.Descending));
			segmentsSource.Refresh();
			_segmentsDataGrid.SelectedIndex = 0;
			_segmentsDataGrid.Columns[1].SortDirection = ListSortDirection.Descending;
		}
	}
}
