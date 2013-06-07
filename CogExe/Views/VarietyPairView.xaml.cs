using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using SIL.Cog.ViewModels;

namespace SIL.Cog.Views
{
	/// <summary>
	/// Interaction logic for VarietyPairView.xaml
	/// </summary>
	public partial class VarietyPairView
	{
		public VarietyPairView()
		{
			InitializeComponent();
		}

		private void VarietyPairView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var vm = DataContext as VarietyPairViewModel;
			if (vm == null)
				return;

			var correspondenceSource = new ListCollectionView(vm.SoundChanges);
			correspondenceSource.GroupDescriptions.Add(new PropertyGroupDescription("Lhs"));
			CorrespondenceDataGrid.ItemsSource = correspondenceSource;
			correspondenceSource.SortDescriptions.Add(new SortDescription("Lhs.Target", ListSortDirection.Ascending));
			correspondenceSource.SortDescriptions.Add(new SortDescription("Probability", ListSortDirection.Descending));
			correspondenceSource.Refresh();
			CorrespondenceDataGrid.SelectedIndex = 0;
		}

		private void CorrespondenceDataGrid_Sorting(object sender, DataGridSortingEventArgs e)
		{
			var lcv = (ListCollectionView) CollectionViewSource.GetDefaultView(CorrespondenceDataGrid.ItemsSource);
			lcv.SortDescriptions.Clear();
			lcv.SortDescriptions.Add(new SortDescription("Lhs.Target", ListSortDirection.Ascending));

			ListSortDirection direction = e.Column.SortDirection != ListSortDirection.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending;

			e.Column.SortDirection = direction;
			string path = null;
			switch ((string) e.Column.Header)
			{
				case "Segment":
					path = "Correspondence";
					break;
				case "Probability":
					path = "Probability";
					break;
				case "Frequency":
					path = "Frequency";
					break;
			}
			lcv.SortDescriptions.Add(new SortDescription(path, direction));
			e.Handled = true;
		}
	}
}
