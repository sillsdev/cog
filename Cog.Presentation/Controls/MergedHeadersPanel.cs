using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using Xceed.Wpf.DataGrid;

namespace SIL.Cog.Presentation.Controls
{
	public class MergedHeadersPanel : Panel
	{
		private static readonly DependencyPropertyKey MergedHeadersPropertyKey = DependencyProperty.RegisterAttachedReadOnly("MergedHeaders", typeof(ObservableCollection<MergedHeader>),
			typeof(MergedHeadersPanel), new FrameworkPropertyMetadata(new ObservableCollection<MergedHeader>()));

		public static readonly DependencyProperty MergedHeadersProperty = MergedHeadersPropertyKey.DependencyProperty;

		public static ObservableCollection<MergedHeader> GetMergedHeaders(DataGridControl dataGrid)
		{
			return (ObservableCollection<MergedHeader>) dataGrid.GetValue(MergedHeadersProperty);
		}

		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);
			DataGridContext ctxt = DataGridControl.GetDataGridContext(this);
			ObservableCollection<MergedHeader> mergedHeaders = GetMergedHeaders(ctxt.DataGridControl);
			mergedHeaders.CollectionChanged += mergedHeaders_CollectionChanged;
			foreach (MergedHeader header in mergedHeaders)
				Children.Add(new MergedHeaderCell {MergedHeader = header, Content = header.Title});
		}

		private void mergedHeaders_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					int addIndex = e.NewStartingIndex;
					foreach (MergedHeader header in e.NewItems)
						Children.Insert(addIndex++, new MergedHeaderCell {MergedHeader = header, Content = header.Title});
					break;
				case NotifyCollectionChangedAction.Remove:
					for (int i = 0; i < e.OldItems.Count; i++)
						Children.RemoveAt(e.OldStartingIndex);
					break;
				case NotifyCollectionChangedAction.Move:
					throw new NotSupportedException();
				case NotifyCollectionChangedAction.Replace:
					int replaceIndex = e.OldStartingIndex;
					foreach (MergedHeader header in e.NewItems)
						Children[replaceIndex++] = new MergedHeaderCell {MergedHeader = header, Content = header.Title};
					break;
				case NotifyCollectionChangedAction.Reset:
					Children.Clear();
					break;
			}
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
			foreach (ColumnBase column in ctxt.VisibleColumns.OrderBy(c => c.VisiblePosition))
			{
				columnOffsets[column.FieldName] = curWidth;
				columnWidths[column.FieldName] = column.ActualWidth;
				curWidth += column.ActualWidth;
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

				cell.Arrange(new Rect(x1, 0, x2 - x1, Math.Max(finalSize.Height, cell.DesiredSize.Height)));
			}

			return finalSize;
		}
	}
}
