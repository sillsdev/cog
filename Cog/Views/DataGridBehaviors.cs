using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace SIL.Cog.Views
{
	public static class DataGridBehaviors
	{
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

			if (element is ComboBoxItem)
			{
				// Since ComboBoxItem.Parent is null, we must pass through ItemsPresenter in order to get the parent ComboBox
				var parentItemsPresenter = FindParentControl<ItemsPresenter>(element as ComboBoxItem);
				var combobox = parentItemsPresenter.TemplatedParent as ComboBox;
				childElement = combobox;
			}
			else
			{
				childElement = element;
			}

			var parentDatagrid = FindParentControl<DataGrid>(childElement);
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
			var parentTabControl = FindParentControl<TabControl>(dataGrid);
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

		private static T FindParentControl<T>(DependencyObject outerDepObj) where T : DependencyObject
		{
			var dObj = VisualTreeHelper.GetParent(outerDepObj);
			if (dObj == null)
			{
				return null;
			}

			if (dObj is T)
			{
				return dObj as T;
			}

			while ((dObj = VisualTreeHelper.GetParent(dObj)) != null)
			{
				if (dObj is T)
				{
					return dObj as T;
				}
			}

			return null;
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
	}
}
