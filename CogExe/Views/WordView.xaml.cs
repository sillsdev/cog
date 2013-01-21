using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using SIL.Cog.ViewModels;

namespace SIL.Cog.Views
{
	/// <summary>
	/// Interaction logic for WordView.xaml
	/// </summary>
	public partial class WordView
	{
		private readonly ItemsControlDrag _drag;
		private readonly ItemsControlDrop _drop;
		private WordSegmentViewModel _prevSelectedItem;

		public WordView()
		{
			InitializeComponent();
			_drag = new ItemsControlDrag(_listBox, CanDrag);
			_drop = new ItemsControlDrop(_listBox, CanDrop);
		}

		private bool CanDrag(FrameworkElement itemContainer)
		{
			return ((WordSegmentViewModel) itemContainer.DataContext).IsBoundary;
		}

		private bool CanDrop(object draggedItem, int index)
		{
			var segs = (IList<WordSegmentViewModel>) _listBox.ItemsSource;
			return (index == segs.Count || !segs[index].IsBoundary) && (index == 0 || !segs[index - 1].IsBoundary);
		}

		private void _listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var vm = (WordSegmentViewModel) _listBox.SelectedItem;
			if (vm == null || vm.IsBoundary)
				_prevSelectedItem = vm;
			else
				_listBox.SelectedItem = _prevSelectedItem;
		}

		private void _listBox_LostFocus(object sender, RoutedEventArgs e)
		{
			_listBox.SelectedItem = null;
		}
	}
}
