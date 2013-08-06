using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using SIL.Cog.Applications.ViewModels;
using SIL.Cog.Presentation.Behaviors;

namespace SIL.Cog.Presentation.Views
{
	/// <summary>
	/// Interaction logic for WordView.xaml
	/// </summary>
	public partial class WordView
	{
		private WordSegmentViewModel _prevSelectedItem;

		public WordView()
		{
			InitializeComponent();
			ListBox.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler((sender, args) => args.Handled = false), true);
		}

		private void _listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var vm = (WordSegmentViewModel) ListBox.SelectedItem;
			if (vm == null || vm.IsBoundary)
				_prevSelectedItem = vm;
			else
				ListBox.SelectedItem = _prevSelectedItem;
			e.Handled = true;
		}

		private void ListBox_OnCanDragItem(object sender, CanDragItemEventArgs e)
		{
			e.CanDrag = ((WordSegmentViewModel) e.ItemContainer.DataContext).IsBoundary;
			e.Handled = true;
		}

		private void ListBox_OnCanDropItem(object sender, CanDropItemEventArgs e)
		{
			var segs = (IList<WordSegmentViewModel>) ListBox.ItemsSource;
			e.CanDrop = (e.Index == segs.Count || !segs[e.Index].IsBoundary) && (e.Index == 0 || !segs[e.Index - 1].IsBoundary);
			e.Handled = true;
		}
	}
}
