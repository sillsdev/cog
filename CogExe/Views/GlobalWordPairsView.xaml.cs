using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SIL.Cog.ViewModels;

namespace SIL.Cog.Views
{
	/// <summary>
	/// Interaction logic for GlobalWordPairsView.xaml
	/// </summary>
	public partial class GlobalWordPairsView
	{
		public GlobalWordPairsView()
		{
			InitializeComponent();
		}

		private void WordPairsListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var vm = (WordPairsViewModel) DataContext;
			foreach (WordPairViewModel wp in e.RemovedItems)
				vm.SelectedWordPairs.Remove(wp);
			foreach (WordPairViewModel wp in e.AddedItems)
				vm.SelectedWordPairs.Add(wp);
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
