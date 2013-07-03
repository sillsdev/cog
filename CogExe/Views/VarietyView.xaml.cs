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
			var vm = DataContext as VarietiesVarietyViewModel;
			if (vm == null)
				return;

			var wordsSource = new ListCollectionView(vm.Words);
			wordsSource.GroupDescriptions.Add(new PropertyGroupDescription("Sense"));
			WordsControl.ItemsSource = wordsSource;
			wordsSource.SortDescriptions.Add(new SortDescription("Sense.Gloss", ListSortDirection.Ascending));
			wordsSource.Refresh();

			var segmentsSource = new ListCollectionView(new ConcurrentList<VarietySegmentViewModel>(vm.Segments));
			SegmentsDataGrid.ItemsSource = segmentsSource;
			segmentsSource.SortDescriptions.Add(new SortDescription("Probability", ListSortDirection.Descending));
			segmentsSource.Refresh();
			SegmentsDataGrid.SelectedIndex = 0;
			SegmentsDataGrid.Columns[1].SortDirection = ListSortDirection.Descending;
		}

		private void MarkerClicked(object sender, MouseButtonEventArgs e)
		{
			var rect = (Rectangle) sender;
			var word = (WordViewModel) rect.DataContext;

			var cp = (ContentPresenter) WordsControl.ItemContainerGenerator.ContainerFromItem(word);
			var point = cp.TransformToAncestor(WordsControl).Transform(new Point());
			ScrollViewer.ScrollToVerticalOffset((point.Y + (cp.ActualHeight / 2)) - (ScrollViewer.ActualHeight / 2));
		}
	}
}
