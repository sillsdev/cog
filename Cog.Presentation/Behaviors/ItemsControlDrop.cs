using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace SIL.Cog.Presentation.Behaviors
{
	public class ItemsControlDrop : IDisposable
	{
		private readonly ItemsControl _target;
		private FrameworkElement _targetItemContainer;
		private bool _hasVerticalOrientation;
		private int _insertionIndex;
		private bool _isInFirstHalf;
		private InsertionAdorner _insertionAdorner;

		public ItemsControlDrop(ItemsControl target)
		{
			_target = target;

			_target.AllowDrop = true;
			_target.PreviewDrop += PreviewDrop;
			_target.PreviewDragEnter += PreviewDragEnter;
			_target.PreviewDragOver += PreviewDragOver;
			_target.PreviewDragLeave += PreviewDragLeave;
		}

		private void PreviewDrop(object sender, DragEventArgs e)
		{
			ItemsControlDragDrop.Instance.EndDragDrop(e, _target, _insertionIndex);
			RemoveInsertionAdorner();
			e.Handled = true;
		}

		private void PreviewDragEnter(object sender, DragEventArgs e)
		{
			DecideDropTarget(e);
			ItemsControlDragDrop.Instance.UpdateDragDrop(e);

			object draggedItem = e.Data.GetData(ItemsControlDragDrop.Format.Name);
			if (draggedItem != null)
				CreateInsertionAdorner();
			e.Handled = true;
		}

		private void PreviewDragOver(object sender, DragEventArgs e)
		{
			DecideDropTarget(e);
			ItemsControlDragDrop.Instance.UpdateDragDrop(e);

			object draggedItem = e.Data.GetData(ItemsControlDragDrop.Format.Name);
			if (draggedItem != null)
				UpdateInsertionAdornerPosition();
			e.Handled = true;
		}

		// If the types of the dragged data and ItemsControl's source are compatible, 
		// there are 3 situations to have into account when deciding the drop target:
		// 1. mouse is over an items container
		// 2. mouse is over the empty part of an ItemsControl, but ItemsControl is not empty
		// 3. mouse is over an empty ItemsControl.
		// The goal of this method is to decide on the values of the following properties: 
		// targetItemContainer, insertionIndex and isInFirstHalf.
		private void DecideDropTarget(DragEventArgs e)
		{
			int targetItemsControlCount = _target.Items.Count;
			object draggedItem = e.Data.GetData(ItemsControlDragDrop.Format.Name);

			if (IsDropDataTypeAllowed(draggedItem))
			{
				if (targetItemsControlCount > 0)
				{
					_hasVerticalOrientation = HasVerticalOrientation(_target.ItemContainerGenerator.ContainerFromIndex(0) as FrameworkElement);
					_targetItemContainer = _target.ContainerFromElement((DependencyObject)e.OriginalSource) as FrameworkElement;

					if (_targetItemContainer != null)
					{
						Point positionRelativeToItemContainer = e.GetPosition(_targetItemContainer);
						_isInFirstHalf = IsInFirstHalf(_targetItemContainer, positionRelativeToItemContainer, _hasVerticalOrientation);
						_insertionIndex = _target.ItemContainerGenerator.IndexFromContainer(_targetItemContainer);

						if (!_isInFirstHalf)
							_insertionIndex++;
					}
					else
					{
						_targetItemContainer = _target.ItemContainerGenerator.ContainerFromIndex(targetItemsControlCount - 1) as FrameworkElement;
						_isInFirstHalf = false;
						_insertionIndex = targetItemsControlCount;
					}
				}
				else
				{
					_targetItemContainer = null;
					_insertionIndex = 0;
				}

				var droppingItem = new CanDropItemEventArgs(draggedItem, _insertionIndex);
				_target.RaiseEvent(droppingItem);

				if (!droppingItem.CanDrop)
				{
					_targetItemContainer = null;
					_insertionIndex = -1;
					e.Effects = DragDropEffects.None;
				}
			}
			else
			{
				_targetItemContainer = null;
				_insertionIndex = -1;
				e.Effects = DragDropEffects.None;
			}
		}

		// Finds the orientation of the panel of the ItemsControl that contains the itemContainer passed as a parameter.
		// The orientation is needed to figure out where to draw the adorner that indicates where the item will be dropped.
		private static bool HasVerticalOrientation(FrameworkElement itemContainer)
		{
			bool hasVerticalOrientation = true;
			if (itemContainer != null)
			{
				var panel = VisualTreeHelper.GetParent(itemContainer) as Panel;
				StackPanel stackPanel;
				WrapPanel wrapPanel;

				if ((stackPanel = panel as StackPanel) != null)
					hasVerticalOrientation = (stackPanel.Orientation == Orientation.Vertical);
				else if ((wrapPanel = panel as WrapPanel) != null)
					hasVerticalOrientation = (wrapPanel.Orientation == Orientation.Vertical);
				// You can add support for more panel types here.
			}
			return hasVerticalOrientation;
		}

		private static bool IsInFirstHalf(FrameworkElement container, Point clickedPoint, bool hasVerticalOrientation)
		{
			if (hasVerticalOrientation)
				return clickedPoint.Y < container.ActualHeight / 2;
			return clickedPoint.X < container.ActualWidth / 2;
		}

		// Can the dragged data be added to the destination collection?
		// It can if destination is bound to IList<allowed type>, IList or not data bound.
		private bool IsDropDataTypeAllowed(object draggedItem)
		{
			bool isDropDataTypeAllowed;
			IEnumerable collectionSource = _target.ItemsSource;
			if (draggedItem != null)
			{
				if (collectionSource != null)
				{
					Type draggedType = draggedItem.GetType();
					Type collectionType = collectionSource.GetType();

					Type genericIListType = collectionType.GetInterface("IList`1");
					if (genericIListType != null)
					{
						Type[] genericArguments = genericIListType.GetGenericArguments();
						isDropDataTypeAllowed = genericArguments[0].IsAssignableFrom(draggedType);
					}
					else if (typeof(IList).IsAssignableFrom(collectionType))
					{
						isDropDataTypeAllowed = true;
					}
					else
					{
						isDropDataTypeAllowed = false;
					}
				}
				else // the ItemsControl's ItemsSource is not data bound.
				{
					isDropDataTypeAllowed = true;
				}
			}
			else
			{
				isDropDataTypeAllowed = false;			
			}
			return isDropDataTypeAllowed;
		}

		private void PreviewDragLeave(object sender, DragEventArgs e)
		{
			// Dragged Adorner is only created once on DragEnter + every time we enter the window. 
			// It's only removed once on the DragDrop, and every time we leave the window. (so no need to remove it here)
			object draggedItem = e.Data.GetData(ItemsControlDragDrop.Format.Name);

			if (draggedItem != null)
				RemoveInsertionAdorner();

			e.Handled = true;
		}

		private void CreateInsertionAdorner()
		{
			if (_targetItemContainer != null)
			{
                // Here, I need to get adorner layer from targetItemContainer and not targetItemsControl. 
				// This way I get the AdornerLayer within ScrollContentPresenter, and not the one under AdornerDecorator (Snoop is awesome).
                // If I used targetItemsControl, the adorner would hang out of ItemsControl when there's a horizontal scroll bar.
				var adornerLayer = AdornerLayer.GetAdornerLayer(_targetItemContainer);
				_insertionAdorner = new InsertionAdorner(_hasVerticalOrientation, _isInFirstHalf, _targetItemContainer, adornerLayer);
			}
		}

		private void UpdateInsertionAdornerPosition()
		{
			if (_insertionAdorner != null)
			{
				_insertionAdorner.IsInFirstHalf = _isInFirstHalf;
				_insertionAdorner.InvalidateVisual();
			}
		}

		private void RemoveInsertionAdorner()
		{
			if (_insertionAdorner != null)
			{
				_insertionAdorner.Detach();
				_insertionAdorner = null;
			}
		}

		public void Dispose()
		{
			_target.AllowDrop = false;
			_target.PreviewDrop -= PreviewDrop;
			_target.PreviewDragEnter -= PreviewDragEnter;
			_target.PreviewDragOver -= PreviewDragOver;
			_target.PreviewDragLeave -= PreviewDragLeave;
		}
	}
}
