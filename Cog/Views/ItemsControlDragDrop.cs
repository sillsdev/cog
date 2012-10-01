using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace SIL.Cog.Views
{
	public class ItemsControlDragDrop
	{
		public static readonly DataFormat Format = DataFormats.GetDataFormat("ItemsControlDragDrop");

		private static ItemsControlDragDrop _instance;
		public static ItemsControlDragDrop Instance
		{
			get
			{
				if (_instance == null)
					_instance = new ItemsControlDragDrop();
				return _instance;
			}
		}

		private Window _topWindow;
		private ItemsControl _source;
		private DraggedAdorner _draggedAdorner;
		private Vector _initialMouseOffset;
		private FrameworkElement _sourceItemContainer;
		private Point _initialMousePosition;

		private readonly Dictionary<ItemsControl, ItemsControlDrag> _dragHelpers;
		private readonly Dictionary<ItemsControl, ItemsControlDrop> _dropHelpers; 

		private ItemsControlDragDrop()
		{
			_dragHelpers = new Dictionary<ItemsControl, ItemsControlDrag>();
			_dropHelpers = new Dictionary<ItemsControl, ItemsControlDrop>();
		}

		public static bool GetIsDragSource(DependencyObject obj)
		{
			return (bool)obj.GetValue(IsDragSourceProperty);
		}

		public static void SetIsDragSource(DependencyObject obj, bool value)
		{
			obj.SetValue(IsDragSourceProperty, value);
		}

		public static readonly DependencyProperty IsDragSourceProperty =
			DependencyProperty.RegisterAttached("IsDragSource", typeof(bool), typeof(ItemsControlDragDrop), new UIPropertyMetadata(false, IsDragSourceChanged));


		public static bool GetIsDropTarget(DependencyObject obj)
		{
			return (bool)obj.GetValue(IsDropTargetProperty);
		}

		public static void SetIsDropTarget(DependencyObject obj, bool value)
		{
			obj.SetValue(IsDropTargetProperty, value);
		}

		public static readonly DependencyProperty IsDropTargetProperty =
			DependencyProperty.RegisterAttached("IsDropTarget", typeof(bool), typeof(ItemsControlDragDrop), new UIPropertyMetadata(false, IsDropTargetChanged));

		public static DataTemplate GetDragDropTemplate(DependencyObject obj)
		{
			return (DataTemplate)obj.GetValue(DragDropTemplateProperty);
		}

		public static void SetDragDropTemplate(DependencyObject obj, DataTemplate value)
		{
			obj.SetValue(DragDropTemplateProperty, value);
		}

		public static readonly DependencyProperty DragDropTemplateProperty =
			DependencyProperty.RegisterAttached("DragDropTemplate", typeof(DataTemplate), typeof(ItemsControlDragDrop), new UIPropertyMetadata(null));

		private static void IsDragSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			var dragSource = obj as ItemsControl;
			if (dragSource != null)
			{
				if (Equals(e.NewValue, true))
				{
					_instance._dragHelpers[dragSource] = new ItemsControlDrag(dragSource);
				}
				else
				{
					ItemsControlDrag drag;
					if (_instance._dragHelpers.TryGetValue(dragSource, out drag))
					{
						drag.Dispose();
						_instance._dragHelpers.Remove(dragSource);
					}
				}
			}
		}

		private static void IsDropTargetChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			var dropTarget = obj as ItemsControl;
			if (dropTarget != null)
			{
				if (Equals(e.NewValue, true))
				{
					_instance._dropHelpers[dropTarget] = new ItemsControlDrop(dropTarget);
				}
				else
				{
					ItemsControlDrop drop;
					if (_instance._dropHelpers.TryGetValue(dropTarget, out drop))
					{
						drop.Dispose();
						_instance._dropHelpers.Remove(dropTarget);
					}
				}
			}
		}

		public void StartDragDrop(ItemsControl source, FrameworkElement sourceItemContainer, object draggedData, Point initialMousePosition)
		{
			_topWindow = Window.GetWindow(source);
			Debug.Assert(_topWindow != null);
			_source = source;
			_sourceItemContainer = sourceItemContainer;
			_initialMousePosition = initialMousePosition;

			_initialMouseOffset = _initialMousePosition - _sourceItemContainer.TranslatePoint(new Point(0, 0), _topWindow);

			var data = new DataObject(Format.Name, draggedData);

			// Adding events to the window to make sure dragged adorner comes up when mouse is not over a drop target.
			bool previousAllowDrop = _topWindow.AllowDrop;
			_topWindow.AllowDrop = true;
			_topWindow.DragEnter += TopWindow_DragEnter;
			_topWindow.DragOver += TopWindow_DragOver;
			_topWindow.DragLeave += TopWindow_DragLeave;
					
			DragDrop.DoDragDrop(_source, data, DragDropEffects.Move);

			// Without this call, there would be a bug in the following scenario: Click on a data item, and drag
			// the mouse very fast outside of the window. When doing this really fast, for some reason I don't get 
			// the Window leave event, and the dragged adorner is left behind.
			// With this call, the dragged adorner will disappear when we release the mouse outside of the window,
			// which is when the DoDragDrop synchronous method returns.
			RemoveDraggedAdorner();

			_topWindow.AllowDrop = previousAllowDrop;
			_topWindow.DragEnter -= TopWindow_DragEnter;
			_topWindow.DragOver -= TopWindow_DragOver;
			_topWindow.DragLeave -= TopWindow_DragLeave;
		}

		public void EndDragDrop(DragEventArgs e, ItemsControl target, int insertionIndex)
		{
			object draggedItem = e.Data.GetData(Format.Name);
			if (draggedItem != null)
			{
				int indexRemoved = -1;
				if ((e.Effects & DragDropEffects.Move) != 0)
				{
					indexRemoved = RemoveItemFromItemsControl(_source, draggedItem);
				}
				// This happens when we drag an item to a later position within the same ItemsControl.
				if (indexRemoved != -1 && _source == target && indexRemoved < insertionIndex)
					insertionIndex--;
				InsertItemInItemsControl(target, draggedItem, insertionIndex);

				RemoveDraggedAdorner();
			}
		}

		private static int RemoveItemFromItemsControl(ItemsControl itemsControl, object itemToRemove)
		{
			int indexToBeRemoved = -1;
			if (itemToRemove != null)
			{
				indexToBeRemoved = itemsControl.Items.IndexOf(itemToRemove);
				
				if (indexToBeRemoved != -1)
				{
					IEnumerable itemsSource = itemsControl.ItemsSource;
					if (itemsSource == null)
					{
						itemsControl.Items.RemoveAt(indexToBeRemoved);
					}
					// Is the ItemsSource IList or IList<T>? If so, remove the item from the list.
					else if (itemsSource is IList)
					{
						((IList)itemsSource).RemoveAt(indexToBeRemoved);
					}
					else
					{
						Type type = itemsSource.GetType();
						Type genericIListType = type.GetInterface("IList`1");
						if (genericIListType != null)
						{
							type.GetMethod("RemoveAt").Invoke(itemsSource, new object[] { indexToBeRemoved });
						}
					}
				}
			}
			return indexToBeRemoved;
		}

		private static void InsertItemInItemsControl(ItemsControl itemsControl, object itemToInsert, int insertionIndex)
		{
			if (itemToInsert != null)
			{
				IEnumerable itemsSource = itemsControl.ItemsSource;

				if (itemsSource == null)
				{
					itemsControl.Items.Insert(insertionIndex, itemToInsert);
				}
				// Is the ItemsSource IList or IList<T>? If so, insert the dragged item in the list.
				else if (itemsSource is IList)
				{
					((IList)itemsSource).Insert(insertionIndex, itemToInsert);
				}
				else
				{
					Type type = itemsSource.GetType();
					Type genericIListType = type.GetInterface("IList`1");
					if (genericIListType != null)
					{
						type.GetMethod("Insert").Invoke(itemsSource, new[] { insertionIndex, itemToInsert });
					}
				}
			}
		}

		private void TopWindow_DragEnter(object sender, DragEventArgs e)
		{
			UpdateDragDrop(e);
			e.Effects = DragDropEffects.None;
			e.Handled = true;
		}

		private void TopWindow_DragOver(object sender, DragEventArgs e)
		{
			UpdateDragDrop(e);
			e.Effects = DragDropEffects.None;
			e.Handled = true;
		}

		private void TopWindow_DragLeave(object sender, DragEventArgs e)
		{
			RemoveDraggedAdorner();
			e.Handled = true;
		}

		public void UpdateDragDrop(DragEventArgs e)
		{
			object draggedData = e.Data.GetData(Format.Name);
			if (draggedData != null)
			{
				Point currentPosition = e.GetPosition(_topWindow);
				if (_draggedAdorner == null)
				{
					var adornerLayer = AdornerLayer.GetAdornerLayer(_source);
					_draggedAdorner = new DraggedAdorner(draggedData, GetDragDropTemplate(_source), _sourceItemContainer, adornerLayer);
				}
				_draggedAdorner.SetPosition(currentPosition.X - _initialMousePosition.X + _initialMouseOffset.X, currentPosition.Y - _initialMousePosition.Y + _initialMouseOffset.Y);
			}
		}

		private void RemoveDraggedAdorner()
		{
			if (_draggedAdorner != null)
			{
				_draggedAdorner.Detach();
				_draggedAdorner = null;
			}
		}
	}
}
