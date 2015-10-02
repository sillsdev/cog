using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using SIL.Cog.Presentation.Behaviors;
using Xceed.Wpf.DataGrid;
using Xceed.Wpf.DataGrid.Views;

namespace SIL.Cog.Presentation.Controls
{
	public class MergedHeadersPanel : Panel
	{
		private readonly MergedHeadersSubPanel _fixedSubPanel;
		private readonly ScrollingMergedHeaderCellDecorator _scrollingDecorator;
		private readonly MergedHeadersSubPanel _scrollingSubPanel;
		private bool _fixedTransformApplied;

		private DataGridContext _context;

		public MergedHeadersPanel()
		{
			_fixedSubPanel = new MergedHeadersSubPanel(true);
			InternalChildren.Add(_fixedSubPanel);

			_scrollingSubPanel = new MergedHeadersSubPanel(false);
			_scrollingDecorator = new ScrollingMergedHeaderCellDecorator {Child = _scrollingSubPanel};
			InternalChildren.Add(_scrollingDecorator);
		}

		public override void EndInit()
		{
			base.EndInit();
			_context = DataGridControl.GetDataGridContext(this);
			_context.DataGridControl.Columns.CollectionChanged += Columns_CollectionChanged;
			AddColumns(_context.DataGridControl.Columns);
			ObservableCollection<MergedHeader> mergedHeaders = DataGridControlBehaviors.GetMergedHeaders(_context.DataGridControl);
			mergedHeaders.CollectionChanged += mergedHeaders_CollectionChanged;
			AddMergedHeaders(mergedHeaders);
		}

		private void Columns_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					AddColumns(e.NewItems.Cast<ColumnBase>());
					break;
				case NotifyCollectionChangedAction.Remove:
					RemoveColumns(e.OldItems.Cast<ColumnBase>());
					break;
				case NotifyCollectionChangedAction.Replace:
					RemoveColumns(e.OldItems.Cast<ColumnBase>());
					AddColumns(e.NewItems.Cast<ColumnBase>());
					break;
				case NotifyCollectionChangedAction.Reset:
					AddColumns((IEnumerable<ColumnBase>) sender);
					break;
			}
		}

		private void AddColumns(IEnumerable<ColumnBase> columns)
		{
			DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(ColumnBase.ActualWidthProperty, typeof(ColumnBase));
			foreach (ColumnBase column in columns)
				dpd.AddValueChanged(column, Column_OnActualWidthChanged);
		}

		private void RemoveColumns(IEnumerable<ColumnBase> columns)
		{
			DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(ColumnBase.ActualWidthProperty, typeof(ColumnBase));
			foreach (ColumnBase column in columns)
				dpd.RemoveValueChanged(column, Column_OnActualWidthChanged);
		}

		private void Column_OnActualWidthChanged(object sender, EventArgs e)
		{
			InvalidateMeasure();
			_fixedSubPanel.InvalidateMeasure();
			_scrollingDecorator.InvalidateMeasure();
		}

		private void mergedHeaders_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					AddMergedHeaders(e.NewItems.Cast<MergedHeader>());
					break;
				case NotifyCollectionChangedAction.Remove:
					RemoveMergedHeaders(e.OldItems.Cast<MergedHeader>());
					break;
				case NotifyCollectionChangedAction.Replace:
					RemoveMergedHeaders(e.OldItems.Cast<MergedHeader>());
					AddMergedHeaders(e.NewItems.Cast<MergedHeader>());
					break;
				case NotifyCollectionChangedAction.Reset:
					_fixedSubPanel.Children.Clear();
					_scrollingSubPanel.Children.Clear();
					break;
			}
		}

		private void AddMergedHeaders(IEnumerable<MergedHeader> mergedHeaders)
		{
			int fixedCount = TableView.GetFixedColumnCount(_context);
			foreach (MergedHeader header in mergedHeaders)
			{
				ColumnBase c = _context.Columns[header.ColumnNames[0]];
				if (c != null)
				{
					MergedHeadersSubPanel subPanel = c.VisiblePosition < fixedCount ? _fixedSubPanel : _scrollingSubPanel;
					subPanel.Children.Add(new MergedHeaderCell {MergedHeader = header, Content = header.Title});
				}
			}
		}

		private void RemoveMergedHeaders(IEnumerable<MergedHeader> mergedHeaders)
		{
			int fixedCount = TableView.GetFixedColumnCount(_context);
			foreach (MergedHeader header in mergedHeaders)
			{
				MergedHeadersSubPanel subPanel = _context.Columns[header.ColumnNames[0]].VisiblePosition < fixedCount ? _fixedSubPanel : _scrollingSubPanel;
				MergedHeaderCell cell = subPanel.Children.Cast<MergedHeaderCell>().Single(c => c.MergedHeader == header);
				subPanel.Children.Remove(cell);
			}
		}

		protected override Size MeasureOverride(Size availableSize)
		{
			if (!_fixedTransformApplied)
			{
				var parentScrollViewer = this.FindVisualAncestor<ScrollViewer>();
				if (parentScrollViewer != null)
				{
					var fixedTranslation = new TranslateTransform();
					var horizontalOffsetBinding = new Binding {Source = parentScrollViewer, Path = new PropertyPath(ScrollViewer.HorizontalOffsetProperty)};
					BindingOperations.SetBinding(fixedTranslation, TranslateTransform.XProperty, horizontalOffsetBinding);
					_fixedSubPanel.RenderTransform = fixedTranslation;
					_scrollingDecorator.RenderTransform = fixedTranslation;
					_fixedTransformApplied = true;
				}
			}

			double width = 0;
			double maxHeight = 0;
			foreach (UIElement subPanel in InternalChildren)
			{
				subPanel.Measure(availableSize);
				width += subPanel.DesiredSize.Width;
				maxHeight = Math.Max(subPanel.DesiredSize.Height, maxHeight);
			}

			return new Size(width, maxHeight);
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			double offset = 0;
			foreach (UIElement subPanel in InternalChildren)
			{
				var rect = new Rect(offset, 0, subPanel.DesiredSize.Width, Math.Max(finalSize.Height, subPanel.DesiredSize.Height));
				subPanel.Arrange(rect);
				offset += rect.Width;
			}

			return finalSize;
		}
	}
}
