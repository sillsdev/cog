using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using SIL.Cog.Presentation.Behaviors;
using Xceed.Wpf.DataGrid;

namespace SIL.Cog.Presentation
{
	public static class PresentationExtensions
	{
		public static bool Validate(this DependencyObject dp)
		{
		   return !Validation.GetHasError(dp) &&
				LogicalTreeHelper.GetChildren(dp)
				.OfType<DependencyObject>()
				.All(Validate);
		}

		public static T FindVisualAncestor<T>(this DependencyObject child) where T : DependencyObject
		{
			DependencyObject parentObj = VisualTreeHelper.GetParent(child);
			if (parentObj == null)
				return null;
			var parent = parentObj as T;
			if (parent != null)
				return parent;
			return FindVisualAncestor<T>(parentObj);
		}

		public static T FindVisualChild<T>(this DependencyObject obj) where T : DependencyObject
		{
			// Search immediate children first (breadth-first)
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
			{
				DependencyObject childObj = VisualTreeHelper.GetChild(obj, i);

				var child = childObj as T;
				if (child != null)
					return child;

				var childOfChild = FindVisualChild<T>(childObj);
				if (childOfChild != null)
					return childOfChild;
			}

			return null;
		}

		public static void SetWidthToFit<T>(this ComboBox comboBox, Func<T, string> stringAccessor)
		{
			double maxWidth = 0;
			foreach (T item in comboBox.ItemsSource)
			{
				string str = stringAccessor(item);
				var formattedText = new FormattedText(str, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
					new Typeface(comboBox.FontFamily, comboBox.FontStyle, comboBox.FontWeight, comboBox.FontStretch), comboBox.FontSize, comboBox.Foreground);
				if (formattedText.Width > maxWidth)
					maxWidth = formattedText.Width;
			}
			comboBox.Width = maxWidth + 25;
		}

		public static void SetWidthToFit<T>(this ColumnBase column, Func<T, string> stringAccessor, double padding)
		{
			DataGridControl dataGrid = column.DataGridControl;
			column.SetWidthToFit(stringAccessor, padding, dataGrid.FontSize);
		}

		public static void SetWidthToFit<T>(this ColumnBase column, Func<T, string> stringAccessor, double padding, double fontSize)
		{
			DataGridControl dataGrid = column.DataGridControl;

			var brush = new SolidColorBrush(Colors.Black);
			double maxWidth = 0;
			foreach (T item in dataGrid.ItemsSource)
			{
				string str = stringAccessor(item);
				var formattedText = new FormattedText(str, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
					new Typeface(dataGrid.FontFamily, dataGrid.FontStyle, dataGrid.FontWeight, dataGrid.FontStretch), fontSize, brush);
				if (formattedText.Width > maxWidth)
					maxWidth = formattedText.Width;
			}
			column.Width = maxWidth + padding;
		}

		public static void SelectFirstCell(this DataGridControl dataGrid)
		{
			dataGrid.Dispatcher.BeginInvoke(new Action(() =>
				{
					if (dataGrid.Items.Count > 0)
					{
						ColumnBase column = dataGrid.VisibleColumns.OrderBy(c => c.VisiblePosition).FirstOrDefault(c => !DataGridControlBehaviors.GetIsRowHeader(c));
						if (column != null)
						{
							dataGrid.SelectedCellRanges.Add(new SelectionCellRange(0, column.Index));
							dataGrid.Focus();
						}
					}
				}), DispatcherPriority.Background);
		}
	}
}
