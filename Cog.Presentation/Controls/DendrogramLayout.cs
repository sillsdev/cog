using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using GalaSoft.MvvmLight.Threading;
using QuickGraph;
using SIL.Cog.Application.ViewModels;

namespace SIL.Cog.Presentation.Controls
{
	public class DendrogramLayout : Canvas
	{
		private Size _desiredSize;
		private readonly Dictionary<HierarchicalGraphVertex, Border> _varietyVertices;
		private readonly Dictionary<HierarchicalGraphVertex, Line> _clusterVertices;
		private readonly Dictionary<HierarchicalGraphEdge, Line> _edges;

		public DendrogramLayout()
		{
			_varietyVertices = new Dictionary<HierarchicalGraphVertex, Border>();
			_clusterVertices = new Dictionary<HierarchicalGraphVertex, Line>();
			_edges = new Dictionary<HierarchicalGraphEdge, Line>();
		}

		public static readonly DependencyProperty GraphProperty = DependencyProperty.Register("Graph",
			typeof(IBidirectionalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge>), typeof(DendrogramLayout),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnGraphPropertyChanged));

		private static void OnGraphPropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
		{
			var dc = (DendrogramLayout)depObj;
			dc.Relayout();
		}

		public IBidirectionalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge> Graph
		{
			get { return (IBidirectionalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge>)GetValue(GraphProperty); }
			set { SetValue(GraphProperty, value); }
		}

		public static readonly DependencyProperty SelectedVertexProperty = DependencyProperty.Register("SelectedVertex",
			typeof(HierarchicalGraphVertex), typeof(DendrogramLayout),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnSelectedVertexPropertyChanged));

		public HierarchicalGraphVertex SelectedVertex
		{
			get { return (HierarchicalGraphVertex)GetValue(SelectedVertexProperty); }
			set { SetValue(SelectedVertexProperty, value); }
		}

		private static void OnSelectedVertexPropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
		{
			var dc = (DendrogramLayout)depObj;
			dc.Relayout();
		}

		private const double OriginX = 3;
		private const double VertexVerticalPadding = 5;

		private void Relayout()
		{
			foreach (HierarchicalGraphVertex vertex in _varietyVertices.Keys)
				vertex.PropertyChanged -= vertex_PropertyChanged;
			_varietyVertices.Clear();
			_edges.Clear();
			Children.Clear();
			_desiredSize = new Size(0, 0);

			if (!IsVisible)
			{
				DependencyPropertyChangedEventHandler handler = null;
				handler = (s, e) =>
					{
						Relayout();
						IsVisibleChanged -= handler;
					};
				IsVisibleChanged += handler;
				return;
			}

			if (Graph == null || Graph.IsVerticesEmpty || SelectedVertex == null)
				return;

			var varietyTextBlocks = new List<TextBlock>();

			double varietyNameWidth = 0;
			double varietyNameHeight = 0;
			foreach (HierarchicalGraphVertex vertex in Graph.Vertices)
			{
				if (!vertex.IsCluster)
				{
					var textBlock = new TextBlock { TextAlignment = TextAlignment.Left, Text = vertex.Name, DataContext = vertex };
					varietyTextBlocks.Add(textBlock);
					var typeface = new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch);
					var text = new FormattedText(vertex.Name, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, textBlock.FontSize, textBlock.Foreground);
					varietyNameWidth = Math.Max(text.Width, varietyNameWidth);
					varietyNameHeight = Math.Max(text.Height, varietyNameHeight);
				}
			}

			double maxDepth = 0;

			foreach (HierarchicalGraphVertex vertex in Graph.Vertices)
				maxDepth = Math.Max(vertex.Depth, maxDepth);

			double textBlocky = VertexVerticalPadding;

			var borderThickness = new Thickness(2);
			double totalHeight = textBlocky + (varietyTextBlocks.Count * (varietyNameHeight + borderThickness.Top + borderThickness.Bottom + VertexVerticalPadding));
			double totalWidth = (varietyNameWidth + borderThickness.Left + borderThickness.Right + 5) + (totalHeight * 1.3);
			_desiredSize = new Size(totalWidth, totalHeight);

			double tickWidth = ((totalWidth - (varietyNameWidth + borderThickness.Left + borderThickness.Right + 5)) - OriginX) / 100;

			var positions = new Dictionary<HierarchicalGraphVertex, Point>();
			foreach (TextBlock varietyTextBlock in varietyTextBlocks)
			{
				var vertex = (HierarchicalGraphVertex)varietyTextBlock.DataContext;
				double tx = 0;
				if (maxDepth > 0)
					tx = tickWidth * ((vertex.Depth * 100) / maxDepth);
				double x = OriginX + tx;
				var border = new Border { BorderThickness = borderThickness, BorderBrush = Brushes.Transparent, Child = varietyTextBlock };
				border.MouseEnter += border_MouseEnter;
				border.MouseLeave += border_MouseLeave;
				SetLeft(border, x);
				SetTop(border, textBlocky);
				Children.Add(border);
				_varietyVertices[vertex] = border;
				vertex.PropertyChanged += vertex_PropertyChanged;
				positions[vertex] = new Point(x, textBlocky + ((varietyNameHeight + borderThickness.Top + borderThickness.Bottom) / 2));
				textBlocky += varietyNameHeight + borderThickness.Top + borderThickness.Bottom + VertexVerticalPadding;
			}

			while (positions.Count < Graph.VertexCount - 1)
			{
				foreach (HierarchicalGraphVertex vertex in Graph.Vertices.Where(v => v != SelectedVertex && !positions.ContainsKey(v)))
					ProcessVertex(positions, tickWidth, maxDepth, vertex);
			}
			ProcessVertex(positions, tickWidth, maxDepth, SelectedVertex);
		}

		private void ProcessVertex(Dictionary<HierarchicalGraphVertex, Point> positions, double tickWidth, double maxDepth, HierarchicalGraphVertex vertex)
		{
			double miny = double.MaxValue;
			double maxy = 0;
			double minx = double.MaxValue;
			double maxx = 0;
			int count = 0;
			foreach (HierarchicalGraphEdge edge in GetEdges(vertex))
			{
				Point p;
				if (!positions.TryGetValue(edge.GetOtherVertex(vertex), out p))
					continue;

				miny = Math.Min(p.Y, miny);
				maxy = Math.Max(p.Y, maxy);

				minx = Math.Min(p.X, minx);
				maxx = Math.Max(p.X, maxx);
				count++;
			}

			if (count >= Graph.Degree(vertex) - 1)
			{
				//double x = originx + (tickWidth * (((offset + vertex.Depth) * 100) / maxDepth));
				double tx = 0;
				if (maxDepth > 0)
					tx = tickWidth * ((vertex.Depth * 100) / maxDepth);
				double x = OriginX + tx;
				double y = (maxy + miny) / 2;

				var vertexLine = new Line { X1 = x, Y1 = miny - 1, X2 = x, Y2 = maxy + 1, Stroke = Brushes.Black, StrokeThickness = 2, DataContext = vertex };
				vertexLine.MouseEnter += vertexLine_MouseEnter;
				vertexLine.MouseLeave += vertexLine_MouseLeave;
				Children.Add(vertexLine);
				_clusterVertices[vertex] = vertexLine;

				foreach (HierarchicalGraphEdge edge in GetEdges(vertex))
				{
					Point p;
					if (!positions.TryGetValue(edge.GetOtherVertex(vertex), out p))
						continue;
					var edgeLine = new Line { X1 = p.X - 1, Y1 = p.Y, X2 = x + 1, Y2 = p.Y, Stroke = Brushes.Black, StrokeThickness = 2, DataContext = edge, ToolTip = string.Format("{0:p}", edge.Length) };
					edgeLine.MouseEnter += edgeLine_MouseEnter;
					edgeLine.MouseLeave += edgeLine_OnMouseLeave;
					Children.Add(edgeLine);
					_edges[edge] = edgeLine;
				}

				positions[vertex] = new Point(x, y);
			}
		}

		private IEnumerable<HierarchicalGraphEdge> GetEdges(HierarchicalGraphVertex vertex)
		{
			IEnumerable<HierarchicalGraphEdge> inEdges;
			if (Graph.TryGetInEdges(vertex, out inEdges))
			{
				foreach (HierarchicalGraphEdge edge in inEdges)
					yield return edge;
			}

			IEnumerable<HierarchicalGraphEdge> outEdges;
			if (Graph.TryGetOutEdges(vertex, out outEdges))
			{
				foreach (HierarchicalGraphEdge edge in outEdges)
					yield return edge;
			}
		}

		private void vertex_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Name")
				DispatcherHelper.CheckBeginInvokeOnUI(Relayout);
		}

		private void edgeLine_MouseEnter(object sender, MouseEventArgs e)
		{
			var line = (Line)sender;
			line.Stroke = Brushes.Yellow;
		}

		private void edgeLine_OnMouseLeave(object sender, MouseEventArgs e)
		{
			var line = (Line)sender;
			line.Stroke = Brushes.Black;
		}

		private void border_MouseEnter(object sender, MouseEventArgs e)
		{
			var border = (Border)sender;
			border.BorderBrush = Brushes.Orange;
		}

		private void border_MouseLeave(object sender, MouseEventArgs e)
		{
			var border = (Border)sender;
			border.BorderBrush = Brushes.Transparent;
		}

		private void vertexLine_MouseEnter(object sender, MouseEventArgs e)
		{
			var line = (Line)sender;
			var vertex = (HierarchicalGraphVertex)line.DataContext;
			HighlightSubtree(vertex, true);
		}

		private void vertexLine_MouseLeave(object sender, MouseEventArgs e)
		{
			var line = (Line)sender;
			var vertex = (HierarchicalGraphVertex)line.DataContext;
			HighlightSubtree(vertex, false);
		}

		private void HighlightSubtree(HierarchicalGraphVertex vertex, bool highlight)
		{
			if (vertex.IsCluster)
			{
				Line line = _clusterVertices[vertex];
				line.Stroke = highlight ? Brushes.Blue : Brushes.Black;
			}
			else
			{
				Border border = _varietyVertices[vertex];
				border.BorderBrush = highlight ? Brushes.Blue : Brushes.Transparent;
			}

			foreach (HierarchicalGraphEdge edge in Graph.OutEdges(vertex))
			{
				Line line = _edges[edge];
				line.Stroke = highlight ? Brushes.Blue : Brushes.Black;
				HighlightSubtree(edge.Target, highlight);
			}
		}

		protected override Size MeasureOverride(Size constraint)
		{
			base.MeasureOverride(constraint);
			return _desiredSize;
		}
	}
}
