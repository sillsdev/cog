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
	/// Interaction logic for WordPairsView.xaml
	/// </summary>
	public partial class WordPairsView
	{
		public WordPairsView()
		{
			InitializeComponent();
			WordPairsListBox.SelectionChanged += WordPairsListBox_SelectionChanged;
		}

		private void WordPairsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var vm = (WordPairsViewModel) DataContext;
			foreach (WordPairViewModel wp in e.RemovedItems)
				vm.SelectedWordPairs.Remove(wp);
			foreach (WordPairViewModel wp in e.AddedItems)
				vm.SelectedWordPairs.Add(wp);
		}

		private void WordPairsView_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var vm = DataContext as WordPairsViewModel;
			if (vm == null)
				return;

			var cognateWordPairSource = new ListCollectionView(vm.WordPairs);
			WordPairsListBox.ItemsSource = cognateWordPairSource;
			cognateWordPairSource.SortDescriptions.Add(new SortDescription("PhoneticSimilarityScore", ListSortDirection.Descending));
			cognateWordPairSource.Refresh();
		}

		private void MarkerClicked(object sender, MouseButtonEventArgs e)
		{
			var rect = (Rectangle) sender;
			ScrollToWordPair((WordPairViewModel) rect.DataContext, ScrollViewer, WordPairsListBox);
		}

		private void ScrollToWordPair(WordPairViewModel wordPair, ScrollViewer sv, ItemsControl ic)
		{
			var cp = (FrameworkElement) ic.ItemContainerGenerator.ContainerFromItem(wordPair);
			var point = cp.TransformToAncestor(ic).Transform(new Point());
			sv.ScrollToVerticalOffset((point.Y + (cp.ActualHeight / 2)) - (sv.ActualHeight / 2));
		}

		private void Copy_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			var vm = (WordPairsViewModel) DataContext;
			Clipboard.SetText(vm.SelectedWordPairsText);
		}

		private void SelectAll_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			WordPairsListBox.SelectAll();
		}

		private void WordPairsListBox_OnLostFocus(object sender, RoutedEventArgs e)
		{
			WordPairsListBox.SelectedItems.Clear();
		}
	}
}
