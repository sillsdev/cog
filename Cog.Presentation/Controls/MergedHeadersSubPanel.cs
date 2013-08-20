using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.DataGrid;
using Xceed.Wpf.DataGrid.Views;

namespace SIL.Cog.Presentation.Controls
{
	public class MergedHeadersSubPanel : Panel
	{
		private readonly bool _fixedCells;
		private ScrollViewer _scrollViewer;

		public MergedHeadersSubPanel(bool fixedCells)
		{
			_fixedCells = fixedCells;
		}

		protected override Size MeasureOverride(Size availableSize)
		{
			DataGridContext ctxt = DataGridControl.GetDataGridContext(this);

			Dictionary<string, double> columnWidths = ctxt.VisibleColumns.ToDictionary(c => c.FieldName, c => c.ActualWidth);
			double width = 0;
			double maxHeight = 0;
			foreach (MergedHeaderCell cell in InternalChildren)
			{
				double headerWidth = cell.MergedHeader.ColumnNames.Sum(name => columnWidths[name]);
				cell.Measure(new Size(headerWidth, availableSize.Height));
				width += headerWidth;
				maxHeight = Math.Max(cell.DesiredSize.Height, maxHeight);
			}

			return new Size(width, maxHeight);
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			DataGridContext ctxt = DataGridControl.GetDataGridContext(this);
			double curWidth = 0;
			var columnWidths = new Dictionary<string, double>();
			var columnOffsets = new Dictionary<string, double>();
			int fixedCount = TableView.GetFixedColumnCount(ctxt);
			IEnumerable<ColumnBase> columns = ctxt.VisibleColumns.OrderBy(c => c.VisiblePosition);
			if (_fixedCells)
				columns = columns.Take(fixedCount);
			else
				columns = columns.Skip(fixedCount);
			foreach (ColumnBase column in columns)
			{
				columnOffsets[column.FieldName] = curWidth;
				columnWidths[column.FieldName] = column.ActualWidth;
				curWidth += column.ActualWidth;
			}

			if (!_fixedCells && _scrollViewer == null)
			{
				_scrollViewer = (ScrollViewer) ctxt.DataGridControl.Template.FindName("PART_ScrollViewer", ctxt.DataGridControl);
				_scrollViewer.ScrollChanged += _scrollViewer_ScrollChanged;
			}

			foreach (MergedHeaderCell cell in InternalChildren)
			{
				double x1 = double.MaxValue;
				double x2 = 0;
				foreach (string columnName in cell.MergedHeader.ColumnNames)
				{
					double offset = columnOffsets[columnName];
					x1 = Math.Min(offset, x1);
					x2 = Math.Max(offset + columnWidths[columnName], x2);
				}

				double width = x2 - x1;
				if (!_fixedCells)
					x1 -= _scrollViewer.HorizontalOffset;
				cell.Arrange(new Rect(x1, 0, width, Math.Max(finalSize.Height, cell.DesiredSize.Height)));
			}

			return finalSize;
		}

		private void _scrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			InvalidateArrange();
		}
	}
}
