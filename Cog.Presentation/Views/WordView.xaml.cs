using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Resources;
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
		private readonly Cursor _closedHandCursor;

		public WordView()
		{
			InitializeComponent();
			ListBox.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler((sender, args) => args.Handled = false), true);
			StreamResourceInfo closedhand = Application.GetResourceStream(new Uri("Images/closedhand.cur", UriKind.Relative));
			Debug.Assert(closedhand != null);
			_closedHandCursor = new Cursor(closedhand.Stream);
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

		private void ListBox_OnGiveFeedback(object sender, GiveFeedbackEventArgs e)
		{
			Mouse.OverrideCursor = null;
			Mouse.SetCursor(_closedHandCursor);
			e.UseDefaultCursors = false;
			e.Handled = true;
		}

		private void ListBox_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			var elem = Mouse.DirectlyOver as FrameworkElement;
			if (elem != null)
			{
				var seg = elem.DataContext as WordSegmentViewModel;
				if (seg != null && seg.IsBoundary)
					Mouse.OverrideCursor = _closedHandCursor;
			}
		}

		private void ListBox_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			Mouse.OverrideCursor = null;
		}
	}
}
