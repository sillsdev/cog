using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using SIL.Cog.Applications.ViewModels;
using SIL.Collections;

namespace SIL.Cog.Presentation.Views
{
	/// <summary>
	/// Interaction logic for WordPairsView.xaml
	/// </summary>
	public partial class WordPairsView
	{
		private readonly SimpleMonitor _monitor;

		public WordPairsView()
		{
			InitializeComponent();
			_monitor = new SimpleMonitor();
		}

		private void WordPairsView_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var vm = DataContext as WordPairsViewModel;
			if (vm == null)
				return;

			vm.SelectedWordPairs.CollectionChanged += SelectedWordPairs_CollectionChanged;
		}

		private void SelectedWordPairs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			var vm = (WordPairsViewModel) DataContext;
			if (_monitor.Busy)
				return;

			using (_monitor.Enter())
			{
				WordPairsListBox.SelectedItems.Clear();
				foreach (WordPairViewModel wordPair in vm.SelectedWordPairs)
					WordPairsListBox.SelectedItems.Add(wordPair);
				if (vm.SelectedWordPairs.Count > 0)
					WordPairsListBox.ScrollIntoView(vm.SelectedWordPairs[0]);
			}
		}

		private void WordPairsListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var vm = (WordPairsViewModel) DataContext;
			if (_monitor.Busy)
				return;

			using (_monitor.Enter())
			{
				foreach (WordPairViewModel wp in e.RemovedItems)
					vm.SelectedWordPairs.Remove(wp);
				foreach (WordPairViewModel wp in e.AddedItems)
					vm.SelectedWordPairs.Add(wp);
			}
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
	}
}
