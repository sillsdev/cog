using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SIL.Cog.Presentation.Behaviors
{
	public class ItemsControlDrag : IDisposable
	{
		private readonly ItemsControl _source;
		private readonly Window _topWindow;
		private Point _initialMousePosition;
		private FrameworkElement _sourceItemContainer;
		private object _draggedData;

		public ItemsControlDrag(ItemsControl source)
		{
			_source = source;

			_topWindow = Window.GetWindow(_source);
			_source.PreviewMouseLeftButtonDown += PreviewMouseLeftButtonDown;
			_source.PreviewMouseMove += PreviewMouseMove;
			_source.PreviewMouseLeftButtonUp += PreviewMouseLeftButtonUp;
		}

		private void PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			var visual = (Visual) e.OriginalSource;

			_initialMousePosition = e.GetPosition(_topWindow);

			_sourceItemContainer = _source.ContainerFromElement(visual) as FrameworkElement;
			if (_sourceItemContainer != null)
				_draggedData = _sourceItemContainer.DataContext;
		}

		private void PreviewMouseMove(object sender, MouseEventArgs e)
		{
			if (_draggedData != null)
			{
				// Only drag when user moved the mouse by a reasonable amount.
				if (IsMovementBigEnough(_initialMousePosition, e.GetPosition(_topWindow)))
				{
					var draggingItem = new CanDragItemEventArgs(_sourceItemContainer);
					_source.RaiseEvent(draggingItem);
					if (draggingItem.CanDrag)
					{
						ItemsControlDragDrop.Instance.StartDragDrop(_source, _sourceItemContainer, _draggedData, _initialMousePosition);
						_draggedData = null;
					}
				}
			}
		}

		private static bool IsMovementBigEnough(Point initialMousePosition, Point currentPosition)
		{
			return (Math.Abs(currentPosition.X - initialMousePosition.X) >= SystemParameters.MinimumHorizontalDragDistance ||
				 Math.Abs(currentPosition.Y - initialMousePosition.Y) >= SystemParameters.MinimumVerticalDragDistance);
		}

		private void PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			_draggedData = null;
		}

		public void Dispose()
		{
			_source.PreviewMouseLeftButtonDown -= PreviewMouseLeftButtonDown;
			_source.PreviewMouseMove -= PreviewMouseMove;
			_source.PreviewMouseLeftButtonUp -= PreviewMouseLeftButtonUp;
		}
	}
}
