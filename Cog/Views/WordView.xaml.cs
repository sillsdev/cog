using System.Collections.Generic;
using System.Windows;
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
	}
}
