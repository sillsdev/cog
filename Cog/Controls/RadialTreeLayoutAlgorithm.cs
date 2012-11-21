using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using GraphSharp.Algorithms.Layout;
using QuickGraph;
using SIL.Cog.ViewModels;

namespace SIL.Cog.Controls
{
	public class RadialTreeLayoutAlgorithm<TVertex, TEdge, TGraph> : DefaultParameterizedLayoutAlgorithmBase<TVertex, TEdge, TGraph, RadialTreeLayoutParameters>
		where TVertex : class
		where TEdge : IEdge<TVertex>
		where TGraph : IBidirectionalGraph<TVertex, TEdge>
	{
		private readonly IDictionary<TVertex, Size> _vertexSizes;
		private readonly Dictionary<TVertex, int> _leafCounts;
		private TVertex _root;
		private double _slope;

		public RadialTreeLayoutAlgorithm(TGraph visitedGraph, IDictionary<TVertex, Point> vertexPositions, IDictionary<TVertex, Size> vertexSizes, RadialTreeLayoutParameters parameters)
			: base(visitedGraph, vertexPositions, parameters)
		{
			_vertexSizes = vertexSizes;
			_leafCounts = new Dictionary<TVertex, int>();
		}

		protected override void InternalCompute()
		{
			_root = VisitedGraph.Vertices.Single(v => VisitedGraph.IsInEdgesEmpty(v));

			_leafCounts.Clear();
			CountLeaves(_root);

			double denom = 2 * Math.Tan(Math.PI / _leafCounts[_root]);
			double minSlope = Parameters.MinimumLength / VisitedGraph.Edges.Min(e => e is ILengthEdge<TVertex> ? ((ILengthEdge<TVertex>) e).Length : 1);
			switch (Parameters.BranchLengthScaling)
			{
				case BranchLengthScaling.MinimizeLabelOverlapAverage:
					bool first = true;
					foreach (TVertex v in VisitedGraph.Vertices.Where(v => VisitedGraph.Degree(v) == 1))
					{
						Size sz = _vertexSizes[v];
						TEdge edge = VisitedGraph.InEdge(v, 0);
						double x = 1;
						var lengthEdge = edge as ILengthEdge<TVertex>;
						if (lengthEdge != null)
							x = lengthEdge.Length;
						double y = sz.Height / denom;
						double slope = y / x;
						_slope = first ? slope : (_slope + slope) / 2;
						first = false;
					}
					_slope = Math.Max(minSlope, _slope);
					break;

				case BranchLengthScaling.MinimizeLabelOverlapMinimum:
					_slope = double.MaxValue;
					foreach (TVertex v in VisitedGraph.Vertices.Where(v => VisitedGraph.Degree(v) == 1))
					{
						Size sz = _vertexSizes[v];
						TEdge edge = VisitedGraph.InEdge(v, 0);
						double x = 1;
						var lengthEdge = edge as ILengthEdge<TVertex>;
						if (lengthEdge != null)
							x = lengthEdge.Length;
						double y = sz.Height / denom;
						_slope = Math.Min(_slope, y / x);
					}
					_slope = Math.Max(minSlope, _slope);
					break;

				case BranchLengthScaling.FixedMinimumLength:
					_slope = minSlope;
					break;
			}

			VertexPositions[_root] = new Point(0, 0);
			CalcPositions(_root, 2 * Math.PI, 0);
		}

		private void CountLeaves(TVertex v)
		{
			if (VisitedGraph.Degree(v) == 1)
			{
				_leafCounts[v] = 1;
			}
			else
			{
				int count = 0;
				foreach (TEdge edge in VisitedGraph.OutEdges(v))
				{
					CountLeaves(edge.Target);
					count += _leafCounts[edge.Target];
				}
				_leafCounts[v] = count;
			}
		}

		private void CalcPositions(TVertex v, double wedgeSize, double wedgeBorderAngle)
		{
			if (v != _root)
			{
				TEdge edge = VisitedGraph.InEdge(v, 0);
				double angle = wedgeBorderAngle + (wedgeSize / 2);
				Size parentSize = _vertexSizes[edge.Source];
				double len = GetLength(edge) + (parentSize.Width / 2) + 5;
				double xDelta = Math.Cos(angle) * len;
				double yDelta = Math.Sin(angle) * len;
				Point parentPoint = VertexPositions[edge.Source];
				VertexPositions[v] = new Point(parentPoint.X + xDelta, parentPoint.Y + yDelta);
				var angledVertex = v as IAngledVertex;
				if (angledVertex != null)
				{
					angledVertex.Angle = (angle * (180 / Math.PI));
					if (angledVertex.Angle > 90 && angledVertex.Angle <= 270)
						angledVertex.Angle -= 180;
				}
			}
			double childWedgeBorderAngle = wedgeBorderAngle;
			foreach (TEdge edge in VisitedGraph.OutEdges(v))
			{
				double childWedgeSize = ((double) _leafCounts[edge.Target] / _leafCounts[_root]) * (2 * Math.PI);
				CalcPositions(edge.Target, childWedgeSize, childWedgeBorderAngle);
				childWedgeBorderAngle += childWedgeSize;
			}
		}

		private double GetLength(TEdge edge)
		{
			double x = 1;
			var lengthEdge = edge as ILengthEdge<TVertex>;
			if (lengthEdge != null)
				x = lengthEdge.Length;
			return _slope * x;
		}
	}
}
