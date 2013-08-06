using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace SIL.Cog.Presentation.Behaviors
{
	public class CanDragItemEventArgs : RoutedEventArgs
	{
		private readonly FrameworkElement _itemContainer;

		public CanDragItemEventArgs(FrameworkElement itemContainer)
			: base(ItemsControlBehaviors.CanDragItemEvent)
		{
			_itemContainer = itemContainer;
			CanDrag = true;
		}

		public FrameworkElement ItemContainer
		{
			get { return _itemContainer; }
		}

		public bool CanDrag { get; set; }
	}

	public delegate void CanDragItemEventHandler(object sender, CanDragItemEventArgs e);

	public class CanDropItemEventArgs : RoutedEventArgs
	{
		private readonly object _draggedItem;
		private readonly int _index;

		public CanDropItemEventArgs(object draggedItem, int index)
			: base(ItemsControlBehaviors.CanDropItemEvent)
		{
			_draggedItem = draggedItem;
			_index = index;
			CanDrop = true;
		}

		public object DraggedItem
		{
			get { return _draggedItem; }
		}

		public int Index
		{
			get { return _index; }
		}

		public bool CanDrop { get; set; }
	}

	public delegate void CanDropItemEventHandler(object sender, CanDropItemEventArgs e);

	public static class ItemsControlBehaviors
	{
		private static readonly Dictionary<ItemsControl, ItemsControlDrag> DragHelpers = new Dictionary<ItemsControl, ItemsControlDrag>();
		private static readonly Dictionary<ItemsControl, ItemsControlDrop> DropHelpers = new Dictionary<ItemsControl, ItemsControlDrop>();

		public static bool GetIsDragSource(DependencyObject obj)
		{
			return (bool)obj.GetValue(IsDragSourceProperty);
		}

		public static void SetIsDragSource(DependencyObject obj, bool value)
		{
			obj.SetValue(IsDragSourceProperty, value);
		}

		public static readonly DependencyProperty IsDragSourceProperty =
			DependencyProperty.RegisterAttached("IsDragSource", typeof(bool), typeof(ItemsControlBehaviors), new UIPropertyMetadata(false, IsDragSourceChanged));


		public static bool GetIsDropTarget(DependencyObject obj)
		{
			return (bool)obj.GetValue(IsDropTargetProperty);
		}

		public static void SetIsDropTarget(DependencyObject obj, bool value)
		{
			obj.SetValue(IsDropTargetProperty, value);
		}

		public static readonly DependencyProperty IsDropTargetProperty =
			DependencyProperty.RegisterAttached("IsDropTarget", typeof(bool), typeof(ItemsControlBehaviors), new UIPropertyMetadata(false, IsDropTargetChanged));

		public static DataTemplate GetDragDropTemplate(DependencyObject obj)
		{
			return (DataTemplate)obj.GetValue(DragDropTemplateProperty);
		}

		public static void SetDragDropTemplate(DependencyObject obj, DataTemplate value)
		{
			obj.SetValue(DragDropTemplateProperty, value);
		}

		public static readonly DependencyProperty DragDropTemplateProperty =
			DependencyProperty.RegisterAttached("DragDropTemplate", typeof(DataTemplate), typeof(ItemsControlBehaviors), new UIPropertyMetadata(null));

		private static void IsDragSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			var dragSource = obj as ItemsControl;
			if (dragSource != null)
			{
				if (Equals(e.NewValue, true))
				{
					DragHelpers[dragSource] = new ItemsControlDrag(dragSource);
				}
				else
				{
					ItemsControlDrag drag;
					if (DragHelpers.TryGetValue(dragSource, out drag))
					{
						drag.Dispose();
						DragHelpers.Remove(dragSource);
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
					DropHelpers[dropTarget] = new ItemsControlDrop(dropTarget);
				}
				else
				{
					ItemsControlDrop drop;
					if (DropHelpers.TryGetValue(dropTarget, out drop))
					{
						drop.Dispose();
						DropHelpers.Remove(dropTarget);
					}
				}
			}
		}

		public static readonly RoutedEvent CanDragItemEvent = EventManager.RegisterRoutedEvent("CanDragItem", RoutingStrategy.Bubble, typeof(CanDragItemEventHandler), typeof(ItemsControlBehaviors));

		public static void AddCanDragItemHandler(DependencyObject obj, CanDragItemEventHandler handler)
		{
			var uie = obj as UIElement;
			if (uie != null)
				uie.AddHandler(CanDragItemEvent, handler);
		}

		public static void RemoveCanDragItemHandler(DependencyObject obj, CanDragItemEventHandler handler)
		{
			var uie = obj as UIElement;
			if (uie != null)
				uie.RemoveHandler(CanDragItemEvent, handler);
		}

		public static readonly RoutedEvent CanDropItemEvent = EventManager.RegisterRoutedEvent("CanDropItem", RoutingStrategy.Bubble, typeof(CanDropItemEventHandler), typeof(ItemsControlBehaviors));

		public static void AddCanDropItemHandler(DependencyObject obj, CanDropItemEventHandler handler)
		{
			var uie = obj as UIElement;
			if (uie != null)
				uie.AddHandler(CanDropItemEvent, handler);
		}

		public static void RemoveCanDropItemHandler(DependencyObject obj, CanDropItemEventHandler handler)
		{
			var uie = obj as UIElement;
			if (uie != null)
				uie.RemoveHandler(CanDropItemEvent, handler);
		}
	}
}
