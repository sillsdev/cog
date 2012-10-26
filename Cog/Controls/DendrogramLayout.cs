using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using GraphSharp;
using SIL.Cog.ViewModels;

namespace SIL.Cog.Controls
{
	public class DendrogramLayout : Canvas
	{
		private Size _desiredSize;
		private bool _relayoutOnVisible;

		public DendrogramLayout()
		{
			IsVisibleChanged += DendrogramLayout_IsVisibleChanged;
		}

		private void DendrogramLayout_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (_relayoutOnVisible && IsVisible)
			{
				Relayout();
				_relayoutOnVisible = false;
			}
		}

		public static readonly DependencyProperty GraphProperty = DependencyProperty.Register("Graph",
			typeof(IHierarchicalBidirectionalGraph<HierarchicalGraphVertex, TypedEdge<HierarchicalGraphVertex>>), typeof(DendrogramLayout),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnGraphPropertyChanged));

		private static void OnGraphPropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
		{
			var dc = (DendrogramLayout) depObj;
			dc.Relayout();
		}

		public IHierarchicalBidirectionalGraph<HierarchicalGraphVertex, TypedEdge<HierarchicalGraphVertex>> Graph
		{
			get { return (IHierarchicalBidirectionalGraph<HierarchicalGraphVertex, TypedEdge<HierarchicalGraphVertex>>) GetValue(GraphProperty); }
			set { SetValue(GraphProperty, value); }
		}

		public static readonly DependencyProperty FixedNodeHeightProperty = DependencyProperty.Register("FixedNodeHeight",
			typeof(bool), typeof(DendrogramLayout), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, OnFixedNodeHeightPropertyChanged));

		private static void OnFixedNodeHeightPropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
		{
			var dc = (DendrogramLayout) depObj;
			dc.Relayout();
		}

		public bool FixedNodeHeight
		{
			get { return (bool) GetValue(FixedNodeHeightProperty); }
			set { SetValue(FixedNodeHeightProperty, value); }
		}

		private int GetDepth(HierarchicalGraphVertex vertex, int curDepth)
		{
			int maxDepth = curDepth;
			foreach (TypedEdge<HierarchicalGraphVertex> edge in Graph.OutEdges(vertex))
			{
				int depth = GetDepth(edge.Target, curDepth + 1);
				maxDepth = Math.Max(depth, maxDepth);
			}
			return maxDepth;
		}

		private void Relayout()
		{
			Children.Clear();
			_desiredSize = new Size(0, 0);

			if (!IsVisible)
			{
				_relayoutOnVisible = true;
				return;
			}

			if (Graph == null || Graph.IsVerticesEmpty)
				return;

			var varieties = new List<Tuple<List<HierarchicalGraphVertex>, TextBlock>>();

			double varietyNameWidth = 0;
			double varietyNameHeight = 0;
			var stack = new Stack<List<HierarchicalGraphVertex>>();
			var vertexGroups = new List<List<HierarchicalGraphVertex>>();
			var vertexGroupDict = new Dictionary<HierarchicalGraphVertex, List<HierarchicalGraphVertex>>();
			foreach (HierarchicalGraphVertex vertex in Graph.Vertices)
			{
				if (vertex.IsCluster)
				{
					if (Graph.InDegree(vertex) == 0)
					{
						var vertexGroup = new List<HierarchicalGraphVertex> {vertex};
						stack.Push(vertexGroup);
						vertexGroups.Add(vertexGroup);
					}
				}
				else
				{
					var textBlock = new TextBlock {Text = vertex.Name, TextAlignment = TextAlignment.Right};
					var vertexGroup = new List<HierarchicalGraphVertex> {vertex};
					vertexGroups.Add(vertexGroup);
					vertexGroupDict[vertex] = vertexGroup;
					varieties.Add(Tuple.Create(vertexGroup, textBlock));
					var typeface = new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch);
					var text = new FormattedText(vertex.Name, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, textBlock.FontSize, textBlock.Foreground);
					varietyNameWidth = Math.Max(text.Width, varietyNameWidth);
					varietyNameHeight = Math.Max(text.Height, varietyNameHeight);
				}
			}

			while (stack.Count > 0)
			{
				List<HierarchicalGraphVertex> vertexGroup = stack.Pop();
				foreach (HierarchicalGraphVertex vertex in vertexGroup.Where(v => !vertexGroupDict.ContainsKey(v)).ToArray())
				{
					foreach (TypedEdge<HierarchicalGraphVertex> edge in Graph.OutEdges(vertex))
					{
						if (!edge.Target.IsCluster)
							continue;

						if (!FixedNodeHeight && Math.Abs(edge.Target.SimilarityScore - vertex.SimilarityScore) < 0.001)
						{
							vertexGroup.Add(edge.Target);
							stack.Push(vertexGroup);
						}
						else
						{
							var childGroup = new List<HierarchicalGraphVertex> {edge.Target};
							vertexGroups.Add(childGroup);
							stack.Push(childGroup);
						}
					}

					vertexGroupDict[vertex] = vertexGroup;
				}
			}

			double textBlockx = varietyNameWidth + 6;

			double originx = textBlockx + 3;

			double maxDist = 0;
			double textBlocky;
			double tickWidth;
			if (FixedNodeHeight)
			{
				int maxDepth = 0;
				foreach (HierarchicalGraphVertex vertex in Graph.Vertices)
				{
					if (Graph.InDegree(vertex) == 0)
						maxDepth = Math.Max(GetDepth(vertex, 0), maxDepth);
				}
				textBlocky = 5;
				double totalHeight = 5 + (varieties.Count * (varietyNameHeight + 5));
				double totalWidth = totalHeight * 1.5;
				_desiredSize = new Size(totalWidth, totalHeight);
				tickWidth = ((_desiredSize.Width - 10) - originx) / maxDepth;
			}
			else
			{
				foreach (HierarchicalGraphVertex vertex in Graph.Vertices)
				{
					if (Graph.InDegree(vertex) == 0)
						maxDist = Math.Max(vertex.SimilarityScore, maxDist);
				}
				double tickLabelHeight = 0;
				double tickLabelWidth = 0;
				var tickLabels = new List<TextBlock>();
				if (maxDist < 0.25)
					maxDist = 0.25;
				else if (maxDist < 0.5)
					maxDist = 0.5;
				else
					maxDist = 1.0;
				for (int i = 0; i < 6; i++)
				{
					double score = i * (maxDist / 5);
					var textBlock = new TextBlock {Text = string.Format("{0:p}", score), TextAlignment = TextAlignment.Center};
					var typeface = new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch);
					var text = new FormattedText(textBlock.Text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, textBlock.FontSize, textBlock.Foreground);
					tickLabels.Add(textBlock);
					tickLabelWidth = Math.Max(text.Width, tickLabelWidth);
					tickLabelHeight = Math.Max(text.Height, tickLabelHeight);
				}

				double totalHeight = tickLabelHeight + 12 + (varieties.Count * (varietyNameHeight + 5));
				double totalWidth = totalHeight * 1.5;
				_desiredSize = new Size(totalWidth, totalHeight);

				tickWidth = ((_desiredSize.Width - 10) - originx) / 100;

				for (int i = 0; i < tickLabels.Count; i++)
				{
					double score = i * (maxDist / 5);
					tickLabels[i].Height = tickLabelHeight;
					tickLabels[i].Width = tickLabelWidth;
					double x = originx + (tickWidth * ((score * 100) / maxDist));
					SetLeft(tickLabels[i], x - (tickLabels[i].Width / 2));
					SetTop(tickLabels[i], 2);
					Children.Add(tickLabels[i]);

					var tickLine = new Line {Stroke = Brushes.Black, StrokeThickness = 1, X1 = x, Y1 = tickLabels[i].Height + 2, X2 = x, Y2 = tickLabels[i].Height + 7};
					Children.Add(tickLine);
				}

				var line = new Line {Stroke = Brushes.Black, StrokeThickness = 1, X1 = originx, Y1 = tickLabelHeight + 7, X2 = _desiredSize.Width - 10, Y2 = tickLabelHeight + 7};
				Children.Add(line);

				textBlocky = tickLabelHeight + 12;
			}

			var positions = new Dictionary<List<HierarchicalGraphVertex>, Point>();
			foreach (Tuple<List<HierarchicalGraphVertex>, TextBlock> variety in varieties)
			{
				variety.Item2.Width = varietyNameWidth;
				variety.Item2.Height = varietyNameHeight;
				SetLeft(variety.Item2, 5);
				SetTop(variety.Item2, textBlocky);
				Children.Add(variety.Item2);
				positions[variety.Item1] = new Point(textBlockx, textBlocky + (varietyNameHeight / 2));
				textBlocky += varietyNameHeight + 5;
			}

			while (positions.Count < vertexGroups.Count)
			{
				foreach (List<HierarchicalGraphVertex> vertexGroup in vertexGroups.Where(vg => !positions.ContainsKey(vg)))
				{
					double miny = double.MaxValue;
					double maxy = 0;
					double minx = double.MaxValue;
					double maxx = 0;
					bool all = true;
					var childGroups = new HashSet<List<HierarchicalGraphVertex>>();
					foreach (HierarchicalGraphVertex vertex in vertexGroup)
					{
						foreach (TypedEdge<HierarchicalGraphVertex> edge in Graph.OutEdges(vertex))
						{
							List<HierarchicalGraphVertex> childGroup = vertexGroupDict[edge.Target];
							if (childGroup == vertexGroup || childGroups.Contains(childGroup))
								continue;

							Point p;
							if (!positions.TryGetValue(childGroup, out p))
							{
								all = false;
								break;
							}

							childGroups.Add(childGroup);

							miny = Math.Min(p.Y, miny);
							maxy = Math.Max(p.Y, maxy);

							minx = Math.Min(p.X, minx);
							maxx = Math.Max(p.X, maxx);
						}

						if (!all)
							break;
					}

					if (all)
					{
						double x = FixedNodeHeight ? maxx + tickWidth : originx + (tickWidth * ((vertexGroup[0].SimilarityScore * 100) / maxDist));
						double y = (maxy + miny) / 2;

						foreach (List<HierarchicalGraphVertex> childGroup in childGroups)
						{
							Point p = positions[childGroup];

							var segments = new PathSegmentCollection
								{
									new LineSegment(new Point(x - minx, p.Y - miny), true),
									new LineSegment(new Point(x - minx, y - miny), Math.Abs(p.Y - miny) < double.Epsilon || Math.Abs(p.Y - maxy) < double.Epsilon)
								};
							var figures = new PathFigureCollection {new PathFigure {StartPoint = new Point(p.X - minx, p.Y - miny), Segments = segments}};
							var geometry = new PathGeometry(figures);

							var path = new Path {StrokeThickness = 1, Stroke = Brushes.Black, Data = geometry};
							SetLeft(path, minx);
							SetTop(path, miny);
							Children.Add(path);
						}

						positions[vertexGroup] = new Point(x, y);
					}
				}
			}
		}

		protected override Size MeasureOverride(Size constraint)
		{
			base.MeasureOverride(constraint);
			return _desiredSize;
		}
	}
}
