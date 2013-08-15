using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using SIL.Cog.Applications.ViewModels;
using SIL.Collections;

namespace SIL.Cog.Presentation.Views
{
	/// <summary>
	/// Interaction logic for GlobalWordPairsView.xaml
	/// </summary>
	public partial class GlobalWordPairsView
	{
		private readonly SimpleMonitor _monitor;

		public GlobalWordPairsView()
		{
			InitializeComponent();
			_monitor = new SimpleMonitor();
		}

		private void GlobalWordPairsView_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var vm = DataContext as WordPairsViewModel;
			if (vm == null)
				return;

			vm.WordPairsView = CollectionViewSource.GetDefaultView(vm.WordPairs);
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
