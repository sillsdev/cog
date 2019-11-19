using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using SIL.Code;
using SIL.Cog.Application.ViewModels;

namespace SIL.Cog.Presentation.Views
{
	/// <summary>
	/// Interaction logic for SegmentsWordsView.xaml
	/// </summary>
	public partial class SegmentsWordsView
	{
		private readonly SimpleMonitor _monitor;

		public SegmentsWordsView()
		{
			InitializeComponent();
			_monitor = new SimpleMonitor();
		}

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var vm = DataContext as WordsViewModel;
			if (vm == null)
				return;

			vm.WordsView = CollectionViewSource.GetDefaultView(vm.Words);
			SelectWords();
			vm.SelectedWords.CollectionChanged += SelectedWords_CollectionChanged;
		}

		private void SelectWords()
		{
			var vm = (WordsViewModel) DataContext;
			if (_monitor.Busy)
				return;
			using (_monitor.Enter())
			{
				foreach (WordViewModel word in WordsListBox.SelectedItems.Cast<WordViewModel>().Except(vm.SelectedWords))
					ClearWordSelection(word);
				WordsListBox.SelectedItems.Clear();
				foreach (WordViewModel word in vm.SelectedWords)
					WordsListBox.SelectedItems.Add(word);
			}
			if (vm.SelectedWords.Count > 0)
				WordsListBox.ScrollIntoView(vm.SelectedWords[0]);
		}

		private void SelectedWords_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			SelectWords();
		}

		private void WordsListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var vm = (WordsViewModel) DataContext;
			if (_monitor.Busy)
				return;

			using (_monitor.Enter())
			{
				foreach (WordViewModel word in e.RemovedItems)
				{
					ClearWordSelection(word);
					vm.SelectedWords.Remove(word);
				}
				foreach (WordViewModel word in e.AddedItems)
					vm.SelectedWords.Add(word);
			}
		}

		private void ClearWordSelection(WordViewModel word)
		{
			var item = (ListBoxItem) WordsListBox.ItemContainerGenerator.ContainerFromItem(word);
			if (item != null)
			{
				ListBox wordListBox = item.FindVisualDescendants<ListBox>().FirstOrDefault();
				if (wordListBox != null)
					wordListBox.UnselectAll();
			}
		}

		private void Copy_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			var vm = (WordsViewModel) DataContext;
			Clipboard.SetText(vm.SelectedWordsText);
		}

		private void SelectAll_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			WordsListBox.SelectAll();
		}

		private void ListBoxItem_OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			var elem = (Border) sender;
			if (!WordsListBox.SelectedItems.Contains(elem.DataContext))
				WordsListBox.SelectedItem = elem.DataContext;
		}
	}
}
