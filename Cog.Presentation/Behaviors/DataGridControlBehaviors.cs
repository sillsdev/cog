using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using SIL.Cog.Application.Collections;
using Xceed.Wpf.DataGrid;
using Xceed.Wpf.DataGrid.Views;

namespace SIL.Cog.Presentation.Behaviors
{
	public static class DataGridControlBehaviors
	{
		public static readonly DependencyProperty IsRowHeaderProperty = DependencyProperty.RegisterAttached("IsRowHeader", typeof(bool), typeof(DataGridControlBehaviors),
			new UIPropertyMetadata(false, OnIsRowHeaderChanged));

		private static void OnIsRowHeaderChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			var c = (ColumnBase) obj;
			if ((bool) e.NewValue)
			{
				c.ReadOnly = true;
				c.CanBeCurrentWhenReadOnly = false;
				c.AllowSort = false;
			}
		}

		public static void SetIsRowHeader(ColumnBase column, bool value)
		{
			column.SetValue(IsRowHeaderProperty, value);
		}

		public static bool GetIsRowHeader(ColumnBase column)
		{
			return (bool) column.GetValue(IsRowHeaderProperty);
		}

		public static readonly DependencyProperty AutoSizeProperty = DependencyProperty.RegisterAttached("AutoSize", typeof(bool), typeof(DataGridControlBehaviors),
			new UIPropertyMetadata(false, OnAutoSizeChanged));

		private static void OnAutoSizeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			var column = (ColumnBase) obj;
			if ((bool) e.NewValue)
			{
				column.PropertyChanged += column_PropertyChanged;
				if (column.DataGridControl != null)
				{
					string valuePath = GetValuePath(column);
					if (valuePath != null)
					{
						SetWidthToFit(column);
						ItemPropertyChangedListener.Subscribe(column, column.DataGridControl.Items, valuePath, DataGridControl_ItemsChanged);
					}
				}
			}
			else
			{
				column.PropertyChanged -= column_PropertyChanged;
				if (column.DataGridControl != null)
					ItemPropertyChangedListener.Unsubscribe(column);
			}
		}

		private static void column_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "DataGridControl")
			{
				var column = (ColumnBase) sender;
				if (column.DataGridControl != null)
				{
					string valuePath = GetValuePath(column);
					if (valuePath != null)
					{
						SetWidthToFit(column);
						ItemPropertyChangedListener.Subscribe(column, column.DataGridControl.Items, valuePath, DataGridControl_ItemsChanged);
					}
				}
				else
				{
					ItemPropertyChangedListener.Unsubscribe(column);
				}
			}
		}

		private static void DataGridControl_ItemsChanged(object obj)
		{
			var column = (ColumnBase) obj;
			SetWidthToFit(column);
		}

		private static string GetValuePath(ColumnBase column)
		{
			var view = column.DataGridControl.ItemsSource as DataGridCollectionView;
			if (view != null)
			{
				var property = (DataGridItemProperty) view.ItemProperties[column.FieldName];
				return property.ValuePath;
			}
			return null;
		}

		private static void SetWidthToFit(ColumnBase column)
		{
			DataGridControl dataGrid = column.DataGridControl;
			double fontSize = GetFontSizeHint(column);
			if (fontSize <= 0.0)
				fontSize = dataGrid.FontSize;
			var brush = new SolidColorBrush(Colors.Black);
			double maxWidth = 0;
			string valuePath = GetValuePath(column);
			foreach (object item in dataGrid.Items)
			{
				object[] values = item.GetPropertyValues(valuePath).ToArray();
				object value = values.Length == 0 ? item : values[values.Length - 1];
				var formattedText = new FormattedText(value.ToString(), CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
					new Typeface(dataGrid.FontFamily, dataGrid.FontStyle, dataGrid.FontWeight, dataGrid.FontStretch), fontSize, brush);
				if (formattedText.Width > maxWidth)
					maxWidth = formattedText.Width;
			}
			column.Width = maxWidth + GetAutoSizePadding(column);
		}

		public static void SetAutoSize(ColumnBase column, bool value)
		{
			column.SetValue(AutoSizeProperty, value);
		}

		public static bool GetAutoSize(ColumnBase column)
		{
			return (bool) column.GetValue(AutoSizeProperty);
		}

		public static readonly DependencyProperty FontSizeHintProperty = DependencyProperty.RegisterAttached("FontSizeHint", typeof(double), typeof(DataGridControlBehaviors),
			new UIPropertyMetadata(0.0));

		public static void SetFontSizeHint(ColumnBase column, double value)
		{
			column.SetValue(FontSizeHintProperty, value);
		}

		public static double GetFontSizeHint(ColumnBase column)
		{
			return (double) column.GetValue(FontSizeHintProperty);
		}

		public static readonly DependencyProperty AutoSizePaddingProperty = DependencyProperty.RegisterAttached("AutoSizePadding", typeof(double), typeof(DataGridControlBehaviors),
			new UIPropertyMetadata(0.0));

		public static void SetAutoSizePadding(ColumnBase column, double value)
		{
			column.SetValue(AutoSizePaddingProperty, value);
		}

		public static double GetAutoSizePadding(ColumnBase column)
		{
			return (double) column.GetValue(AutoSizePaddingProperty);
		}

		public static readonly DependencyProperty MergedHeadersProperty = DependencyProperty.RegisterAttached("MergedHeadersInternal", typeof(ObservableCollection<MergedHeader>),
			typeof(DataGridControlBehaviors), new FrameworkPropertyMetadata(null));

		public static void SetMergedHeaders(DataGridControl dataGrid, ObservableCollection<MergedHeader> headers)
		{
			dataGrid.SetValue(MergedHeadersProperty, headers);
		}
		
		public static ObservableCollection<MergedHeader> GetMergedHeaders(DataGridControl dataGrid)
		{
			var headers = (ObservableCollection<MergedHeader>) dataGrid.GetValue(MergedHeadersProperty);
			if (headers == null)
			{
				headers = new ObservableCollection<MergedHeader>();
				dataGrid.SetValue(MergedHeadersProperty, headers);
			}

			return headers;
		}

		public static readonly DependencyProperty IsRowVirtualizationEnabledProperty = DependencyProperty.RegisterAttached("IsRowVirtualizationEnabled", typeof(bool), typeof(DataGridControlBehaviors),
			new UIPropertyMetadata(true, OnIsRowVirtualizationEnabledChanged));

		private static void OnIsRowVirtualizationEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			var dataGrid = (DataGridControl) obj;
			if (dataGrid.IsLoaded)
			{
				SetRowVirtualization(dataGrid);
			}
			else
			{
				if (!((bool) e.NewValue))
					dataGrid.Loaded += dataGrid_Loaded;
				else
					dataGrid.Loaded -= dataGrid_Loaded;
			}
		}

		private static void dataGrid_Loaded(object sender, RoutedEventArgs e)
		{
			var dataGrid = (DataGridControl) sender;
			SetRowVirtualization(dataGrid);
			dataGrid.Loaded -= dataGrid_Loaded;
		}

		private static void SetRowVirtualization(DataGridControl dataGrid)
		{
			var scrollViewer = (TableViewScrollViewer) dataGrid.Template.FindName("PART_ScrollViewer", dataGrid);
			scrollViewer.CanContentScroll = GetIsRowVirtualizationEnabled(dataGrid);
		}

		public static void SetIsRowVirtualizationEnabled(DataGridControl dataGrid, bool value)
		{
			dataGrid.SetValue(IsRowVirtualizationEnabledProperty, value);
		}

		public static bool GetIsRowVirtualizationEnabled(DataGridControl dataGrid)
		{
			return (bool) dataGrid.GetValue(IsRowVirtualizationEnabledProperty);
		}

		public static readonly DependencyProperty AutoScrollOnSelectionProperty =
			DependencyProperty.RegisterAttached(
				"AutoScrollOnSelection", 
				typeof(bool), 
				typeof(DataGridControlBehaviors), 
				new UIPropertyMetadata(false, OnAutoScrollOnSelectionChanged));

		private static void OnAutoScrollOnSelectionChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
		{
			var dataGrid = depObj as DataGridControl;
			if (dataGrid == null)
				return;

			if ((bool) e.NewValue)
				dataGrid.SelectionChanged += DataGrid_SelectionChanged;
			else
				dataGrid.SelectionChanged -= DataGrid_SelectionChanged;
		}

		private static void DataGrid_SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
		{
			var dataGrid = (DataGridControl) sender;
			if (dataGrid != null && dataGrid.IsLoaded && dataGrid.SelectedItem != null)
			{
				dataGrid.Focus();
				dataGrid.Dispatcher.BeginInvoke(new Action(() => dataGrid.BringItemIntoView(dataGrid.SelectedItem)), DispatcherPriority.Background);
			}
		}

		public static bool GetAutoScrollOnSelection(DataGridControl datagrid)
		{
			return (bool) datagrid.GetValue(AutoScrollOnSelectionProperty);
		}

		public static void SetAutoScrollOnSelection(DataGridControl datagrid, bool value)
		{
			datagrid.SetValue(AutoScrollOnSelectionProperty, value);
		}

		public static readonly DependencyProperty IsInitialSelectionDisabledProperty =
			DependencyProperty.RegisterAttached(
				"IsInitialSelectionDisabled", 
				typeof(bool), 
				typeof(DataGridControlBehaviors), 
				new UIPropertyMetadata(false, OnIsInitialSelectionDisabledChanged));

		private static void OnIsInitialSelectionDisabledChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
		{
			var dataGrid = depObj as DataGridControl;
			if (dataGrid == null)
				return;

			if ((bool) e.NewValue)
				dataGrid.ItemsSourceChangeCompleted += DataGrid_ItemsSourceChangeCompleted;
			else
				dataGrid.ItemsSourceChangeCompleted -= DataGrid_ItemsSourceChangeCompleted;
		}

		private static void DataGrid_ItemsSourceChangeCompleted(object sender, EventArgs e)
		{
			var dataGrid = (DataGridControl) sender;
			switch (dataGrid.SelectionUnit)
			{
				case SelectionUnit.Cell:
					dataGrid.Dispatcher.BeginInvoke(new Action(() =>
						{
							dataGrid.CurrentColumn = null;
							dataGrid.CurrentItem = null;
						}));
					break;
				case SelectionUnit.Row:
					dataGrid.SelectedItems.Clear();
					break;
			}
		}

		public static void SetIsInitialSelectionDisabled(DataGridControl dataGrid, bool value)
		{
			dataGrid.SetValue(IsInitialSelectionDisabledProperty, value);
		}

		public static bool GetIsInitialSelectionDisabled(DataGridControl dataGrid)
		{
			return (bool) dataGrid.GetValue(IsInitialSelectionDisabledProperty);
		}

		public static readonly DependencyProperty IsUnselectableProperty =
			DependencyProperty.RegisterAttached(
				"IsUnselectable", 
				typeof(bool), 
				typeof(DataGridControlBehaviors), 
				new UIPropertyMetadata(false, OnIsUnselectableChanged));

		private static void OnIsUnselectableChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
		{
			var dataGrid = depObj as DataGridControl;
			if (dataGrid == null)
				return;

			if ((bool) e.NewValue)
				dataGrid.PreviewMouseLeftButtonUp += DataGrid_PreviewMouseLeftButtonUp;
			else
				dataGrid.PreviewMouseLeftButtonUp -= DataGrid_PreviewMouseLeftButtonUp;
		}

		private static void DataGrid_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			var dataGrid = (DataGridControl) sender;
			if (dataGrid.SelectionMode == SelectionMode.Single && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
			{
				var elem = (DependencyObject) e.OriginalSource;
				var visual = elem as Visual;
				if (visual == null)
					visual = elem.FindLogicalAncestors<Visual>().First();

				switch (dataGrid.SelectionUnit)
				{
					case SelectionUnit.Cell:
						DataCell cell = visual.FindVisualAncestors<DataCell>().First();
						if (cell.IsSelected)
						{
							dataGrid.SelectedCellRanges.Clear();
							dataGrid.CurrentColumn = null;
							dataGrid.CurrentItem = null;
						}
						break;

					case SelectionUnit.Row:
						DataRow row = visual.FindVisualAncestors<DataRow>().First();
						if (row.IsSelected)
						{
							dataGrid.SelectedItem = null;
							dataGrid.CurrentColumn = null;
							dataGrid.CurrentItem = null;
						}
						break;
				}
				e.Handled = true;
			}
		}

		public static void SetIsUnselectable(DataGridControl dataGrid, bool value)
		{
			dataGrid.SetValue(IsUnselectableProperty, value);
		}

		public static bool GetIsUnselectable(DataGridControl dataGrid)
		{
			return (bool) dataGrid.GetValue(IsUnselectableProperty);
		}

		public static readonly DependencyProperty AllowCurrentWhenNoSelectionProperty =
			DependencyProperty.RegisterAttached(
				"AllowCurrentWhenNoSelection", 
				typeof(bool), 
				typeof(DataGridControlBehaviors), 
				new UIPropertyMetadata(true, OnAllowCurrentWhenNoSelectionChanged));

		private static void OnAllowCurrentWhenNoSelectionChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
		{
			var dataGrid = depObj as DataGridControl;
			if (dataGrid == null)
				return;

			if (!((bool) e.NewValue))
				dataGrid.PropertyChanged += DataGrid_PropertyChanged;
			else
				dataGrid.PropertyChanged -= DataGrid_PropertyChanged;
		}

		private static void DataGrid_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var dataGrid = (DataGridControl) sender;
			switch (e.PropertyName)
			{
				case "CurrentItem":
					if (dataGrid.CurrentItem == null)
						return;
					dataGrid.Dispatcher.BeginInvoke(new Action(() =>
						{
							if (dataGrid.SelectedCellRanges.Count == 0)
								dataGrid.CurrentItem = null;
						}));
					break;

				case "CurrentColumn":
					if (dataGrid.CurrentColumn == null)
						return;
					dataGrid.Dispatcher.BeginInvoke(new Action(() =>
						{
							if (dataGrid.SelectedCellRanges.Count == 0)
								dataGrid.CurrentColumn = null;
						}));
					break;
			}
		}

		public static void SetAllowCurrentWhenNoSelection(DataGridControl dataGrid, bool value)
		{
			dataGrid.SetValue(AllowCurrentWhenNoSelectionProperty, value);
		}

		public static bool GetAllowCurrentWhenNoSelection(DataGridControl dataGrid)
		{
			return (bool) dataGrid.GetValue(AllowCurrentWhenNoSelectionProperty);
		}
	}
}
