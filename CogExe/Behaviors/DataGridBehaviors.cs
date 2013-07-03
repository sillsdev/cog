using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using SIL.Cog.Views;
using SIL.Collections;

namespace SIL.Cog.Behaviors
{
	public static class DataGridBehaviors
	{
		static DataGridBehaviors()
		{
			FrameworkElement.DataContextProperty.AddOwner(typeof(DataGridColumn));
		}

		public static readonly DependencyProperty CommitOnLostFocusProperty =
			DependencyProperty.RegisterAttached(
				"CommitOnLostFocus", 
				typeof(bool), 
				typeof(DataGridBehaviors), 
				new UIPropertyMetadata(false, OnCommitOnLostFocusChanged));

		/// <summary>
		///   A hack to find the data grid in the event handler of the tab control.
		/// </summary>
		private static readonly Dictionary<TabPanel, DataGrid> ControlMap = new Dictionary<TabPanel, DataGrid>();
		private static readonly Dictionary<DataGrid, SimpleMonitor> Monitors = new Dictionary<DataGrid, SimpleMonitor>(); 

		public static bool GetCommitOnLostFocus(DataGrid datagrid)
		{
			return (bool) datagrid.GetValue(CommitOnLostFocusProperty);
		}

		public static void SetCommitOnLostFocus(DataGrid datagrid, bool value)
		{
			datagrid.SetValue(CommitOnLostFocusProperty, value);
		}

		private static void CommitEdit(DataGrid dataGrid)
		{
			dataGrid.CommitEdit(DataGridEditingUnit.Cell, true);
			dataGrid.CommitEdit(DataGridEditingUnit.Row, true);
		}

		private static DataGrid GetParentDatagrid(UIElement element)
		{
			UIElement childElement; // element from which to start the tree navigation, looking for a Datagrid parent

			var comboBoxItem = element as ComboBoxItem;
			if (comboBoxItem != null)
			{
				// Since ComboBoxItem.Parent is null, we must pass through ItemsPresenter in order to get the parent ComboBox
				var parentItemsPresenter = comboBoxItem.FindVisualAncestor<ItemsPresenter>();
				var combobox = (ComboBox) parentItemsPresenter.TemplatedParent;
				childElement = combobox;
			}
			else
			{
				childElement = element;
			}

			var parentDatagrid = childElement.FindVisualAncestor<DataGrid>();
			return parentDatagrid;
		}

		private static TabPanel GetTabPanel(TabControl tabControl)
		{
			return (TabPanel) tabControl.GetType().InvokeMember("ItemsHost", BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.Instance, null, tabControl, null);
		}

		private static void OnCommitOnLostFocusChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
		{
			var dataGrid = depObj as DataGrid;
			if (dataGrid == null)
				return;

			if (!(e.NewValue is bool))
				return;

			if ((bool) e.NewValue)
			{
				dataGrid.LostKeyboardFocus += OnDataGridLostFocus;
				dataGrid.DataContextChanged += OnDataGridDataContextChanged;
				dataGrid.IsVisibleChanged += OnDataGridIsVisibleChanged;
				dataGrid.Loaded += OnDataGridLoaded;
				dataGrid.CellEditEnding += OnDataGridCellEditEnding;
				Monitors[dataGrid] = new SimpleMonitor();
			}
			else
			{
				TabPanel tabPanel = FindTabPanel(dataGrid);
				if (tabPanel != null)
				{
					ControlMap.Remove(tabPanel);
					tabPanel.PreviewMouseLeftButtonDown -= OnParentTabControlPreviewMouseLeftButtonDown;
				}

				dataGrid.LostKeyboardFocus -= OnDataGridLostFocus;
				dataGrid.DataContextChanged -= OnDataGridDataContextChanged;
				dataGrid.IsVisibleChanged -= OnDataGridIsVisibleChanged;
				dataGrid.Loaded -= OnDataGridLoaded;
				dataGrid.CellEditEnding -= OnDataGridCellEditEnding;
				Monitors.Remove(dataGrid);
			}
		}

		private static void OnDataGridCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
		{
			var dataGrid = (DataGrid) sender;
			if (!Monitors[dataGrid].Busy && e.EditAction == DataGridEditAction.Commit)
			{
				using (Monitors[dataGrid].Enter())
					CommitEdit(dataGrid);
			}
		}

		private static void OnDataGridLoaded(object sender, RoutedEventArgs e)
		{
			var dataGrid = (DataGrid) sender;
			TabPanel tabPanel = FindTabPanel(dataGrid);
			if (tabPanel != null)
			{
				ControlMap[tabPanel] = dataGrid;
				tabPanel.PreviewMouseLeftButtonDown += OnParentTabControlPreviewMouseLeftButtonDown;
			}
		}

		private static TabPanel FindTabPanel(DataGrid dataGrid)
		{
			var parentTabControl = dataGrid.FindVisualAncestor<TabControl>();
			if (parentTabControl != null)
				return GetTabPanel(parentTabControl);
			return null;
		}

		private static void OnDataGridDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var dataGrid = (DataGrid) sender;
			CommitEdit(dataGrid);
		}

		private static void OnDataGridIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var senderDatagrid = (DataGrid) sender;

			if ((bool) e.NewValue == false)
			{
				CommitEdit(senderDatagrid);
			}
		}

		private static void OnDataGridLostFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			var dataGrid = (DataGrid) sender;

			var focusedElement = Keyboard.FocusedElement as UIElement;
			if (focusedElement == null)
				return;

			DataGrid focusedDatagrid = GetParentDatagrid(focusedElement);

			// Let's see if the new focused element is inside a datagrid
			if (focusedDatagrid == dataGrid)
			{
				// If the new focused element is inside the same datagrid, then we don't need to do anything;
				// this happens, for instance, when we enter in edit-mode: the DataGrid element loses keyboard-focus, 
				// which passes to the selected DataGridCell child
				return;
			}

			CommitEdit(dataGrid);
		}

		private static void OnParentTabControlPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			var dataGrid = ControlMap[(TabPanel) sender];
			CommitEdit(dataGrid);
		}

		public static readonly DependencyProperty SelectAllButtonStyleProperty =
			DependencyProperty.RegisterAttached(
				"SelectAllButtonStyle", 
				typeof(Style), 
				typeof(DataGridBehaviors), 
				new UIPropertyMetadata(null, OnSelectAllButtonStyleChanged));

		private static void OnSelectAllButtonStyleChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
		{
			var dataGrid = depObj as DataGrid;
			if (dataGrid == null)
				return;

			var style = e.NewValue as Style;

			if (style != null)
				dataGrid.Loaded += DataGrid_Loaded;
			else
				dataGrid.Loaded -= DataGrid_Loaded;
		}

		public static Style GetSelectAllButtonStyle(DataGrid datagrid)
		{
			return (Style) datagrid.GetValue(SelectAllButtonStyleProperty);
		}

		public static void SetSelectAllButtonStyle(DataGrid datagrid, Style value)
		{
			datagrid.SetValue(SelectAllButtonStyleProperty, value);
		}

		private static void DataGrid_Loaded(object sender, EventArgs e)
		{
			var dataGrid = (DataGrid) sender;
            DependencyObject dep = dataGrid;
            while (dep != null && VisualTreeHelper.GetChildrenCount(dep) != 0
                && !(dep is Button && ((Button) dep).Command == DataGrid.SelectAllCommand))
            {
                dep = VisualTreeHelper.GetChild(dep, 0);
            }
 
            var button = dep as Button;
            if (button != null)
	            button.Style = GetSelectAllButtonStyle(dataGrid);
		}

		public static readonly DependencyProperty AutoScrollOnSelectionProperty =
			DependencyProperty.RegisterAttached(
				"AutoScrollOnSelection", 
				typeof(bool), 
				typeof(DataGridBehaviors), 
				new UIPropertyMetadata(false, OnAutoScrollOnSelectionChanged));

		private static void OnAutoScrollOnSelectionChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
		{
			var dataGrid = depObj as DataGrid;
			if (dataGrid == null)
				return;

			if (!(e.NewValue is bool))
				return;

			if ((bool) e.NewValue)
				dataGrid.SelectionChanged += DataGrid_SelectionChanged;
			else
				dataGrid.SelectionChanged -= DataGrid_SelectionChanged;
		}

		private static void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			// Only react to the SelectionChanged event raised by the DataGrid
			// Ignore all ancestors.
			if (sender != e.OriginalSource)
				return;

			var dataGrid = e.OriginalSource as DataGrid;
			if (dataGrid != null && dataGrid.SelectedItem != null)
				dataGrid.ScrollIntoView(dataGrid.SelectedItem);
		}

		public static bool GetAutoScrollOnSelection(DataGrid datagrid)
		{
			return (bool) datagrid.GetValue(AutoScrollOnSelectionProperty);
		}

		public static void SetAutoScrollOnSelection(DataGrid datagrid, bool value)
		{
			datagrid.SetValue(AutoScrollOnSelectionProperty, value);
		}

		public static object GetDataContextForColumns(DependencyObject obj)
		{
			return obj.GetValue(DataContextForColumnsProperty);
		}
 
		public static void SetDataContextForColumns(DependencyObject obj, object value)
		{
			obj.SetValue(DataContextForColumnsProperty, value);
		}
 
		/// <summary>
		/// Allows to set DataContext property on columns of the DataGrid (DataGridColumn)
		/// </summary>
		/// <example><DataGridTextColumn Header="{Binding DataContext.ColumnHeader, RelativeSource={RelativeSource Self}}" /></example>
		public static readonly DependencyProperty DataContextForColumnsProperty =
			DependencyProperty.RegisterAttached(
			"DataContextForColumns",
			typeof(object),
			typeof(DataGridBehaviors),
			new UIPropertyMetadata(OnDataContextChanged));
 
		/// <summary>
		/// Propogates the context change to all the DataGrid's columns
		/// </summary>
		private static void OnDataContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var grid = d as DataGrid;
			if (grid == null) return;
 
			foreach (DataGridColumn col in grid.Columns)
				col.SetValue(FrameworkElement.DataContextProperty, e.NewValue);
		}
	}
}
