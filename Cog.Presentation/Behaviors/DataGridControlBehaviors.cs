using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
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

		private static readonly DependencyPropertyKey MergedHeadersPropertyKey = DependencyProperty.RegisterAttachedReadOnly("MergedHeaders", typeof(ObservableCollection<MergedHeader>),
			typeof(DataGridControlBehaviors), new FrameworkPropertyMetadata(new ObservableCollection<MergedHeader>()));

		public static readonly DependencyProperty MergedHeadersProperty = MergedHeadersPropertyKey.DependencyProperty;

		public static ObservableCollection<MergedHeader> GetMergedHeaders(DataGridControl dataGrid)
		{
			return (ObservableCollection<MergedHeader>) dataGrid.GetValue(MergedHeadersProperty);
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

		public static readonly DependencyProperty IsGroupingProperty = DependencyProperty.RegisterAttached("IsGrouping", typeof(bool), typeof(DataGridControlBehaviors),
			new UIPropertyMetadata(false, OnIsGroupingChanged));

		private static void OnIsGroupingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			var dataGrid = (DataGridControl) obj;
			if ((bool) e.NewValue)
				((INotifyCollectionChanged) dataGrid.Items.Groups).CollectionChanged += GroupsChanged;
			else
				((INotifyCollectionChanged) dataGrid.Items.Groups).CollectionChanged -= GroupsChanged;
		}

		private static void GroupsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{

		}

		public static void SetIsGrouping(DataGridControl dataGrid, bool value)
		{
			dataGrid.SetValue(IsGroupingProperty, value);
		}

		public static bool GetIsGrouping(DataGridControl dataGrid)
		{
			return (bool) dataGrid.GetValue(IsGroupingProperty);
		}

		private static readonly DependencyPropertyKey IsFirstInGroupPropertyKey = DependencyProperty.RegisterAttachedReadOnly("IsFirstInGroup", typeof(bool),
			typeof(DataGridControlBehaviors), new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty IsFirstInGroupProperty = IsFirstInGroupPropertyKey.DependencyProperty;

		public static bool GetIsFirstInGroup(DependencyObject obj)
		{
			return (bool) obj.GetValue(IsFirstInGroupProperty);
		}
	}
}
