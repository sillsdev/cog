using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Shapes;
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
			CorrespondenceDataGrid.ItemsSource = correspondenceSource;
			correspondenceSource.SortDescriptions.Add(new SortDescription("Lhs.Target", ListSortDirection.Ascending));
			correspondenceSource.SortDescriptions.Add(new SortDescription("Probability", ListSortDirection.Descending));
			correspondenceSource.Refresh();
			CorrespondenceDataGrid.SelectedIndex = 0;

			var cognateWordPairSource = new ListCollectionView(vm.WordPairs) {Filter = item => ((WordPairViewModel) item).AreCognate};
			CognateWordPairsControl.ItemsSource = cognateWordPairSource;
			cognateWordPairSource.SortDescriptions.Add(new SortDescription("PhoneticSimilarityScore", ListSortDirection.Descending));
			cognateWordPairSource.Refresh();

			var noncognateWordPairSource = new ListCollectionView(vm.WordPairs) {Filter = item => !((WordPairViewModel) item).AreCognate};
			NoncognateWordPairsControl.ItemsSource = noncognateWordPairSource;
			noncognateWordPairSource.SortDescriptions.Add(new SortDescription("PhoneticSimilarityScore", ListSortDirection.Descending));
			noncognateWordPairSource.Refresh();
		}

		private void CognateMarkerClicked(object sender, MouseButtonEventArgs e)
		{
			var rect = (Rectangle) sender;
			ScrollToWordPair((WordPairViewModel) rect.DataContext, CognateScrollViewer, CognateWordPairsControl);
		}

		private void NoncognateMarkerClicked(object sender, MouseButtonEventArgs e)
		{
			var rect = (Rectangle) sender;
			ScrollToWordPair((WordPairViewModel) rect.DataContext, NoncognateScrollViewer, NoncognateWordPairsControl);
		}

		private void ScrollToWordPair(WordPairViewModel wordPair, ScrollViewer sv, ItemsControl ic)
		{
			var cp = (ContentPresenter) ic.ItemContainerGenerator.ContainerFromItem(wordPair);
			var point = cp.TransformToAncestor(ic).Transform(new Point());
			sv.ScrollToVerticalOffset((point.Y + (cp.ActualHeight / 2)) - (sv.ActualHeight / 2));
		}

		private void CognateSelectedWordPairsFilter(object sender, FilterEventArgs e)
		{
			e.Accepted = ((WordPairViewModel) e.Item).AreCognate;
		}

		private void NoncognateSelectedWordPairsFilter(object sender, FilterEventArgs e)
		{
			e.Accepted = !((WordPairViewModel) e.Item).AreCognate;
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
