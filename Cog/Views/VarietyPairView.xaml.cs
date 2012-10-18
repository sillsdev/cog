using System.ComponentModel;
using System.Windows;
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

			var correspondenceSource = new ListCollectionView(vm.Correspondences);
			correspondenceSource.GroupDescriptions.Add(new PropertyGroupDescription("Lhs"));
			_correspondenceDataGrid.ItemsSource = correspondenceSource;
			correspondenceSource.SortDescriptions.Add(new SortDescription("Lhs.Target", ListSortDirection.Ascending));
			correspondenceSource.SortDescriptions.Add(new SortDescription("Probability", ListSortDirection.Descending));
			correspondenceSource.Refresh();
			_correspondenceDataGrid.SelectedIndex = 0;

			var wordPairSource = new ListCollectionView(vm.WordPairs);
			wordPairSource.GroupDescriptions.Add(new PropertyGroupDescription("AreCognate"));
			_wordPairsControl.ItemsSource = wordPairSource;
			wordPairSource.SortDescriptions.Add(new SortDescription("PhoneticSimilarityScore", ListSortDirection.Descending));
			wordPairSource.Refresh();
		}
	}
}
