using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using GraphSharp.Algorithms.EdgeRouting;
using QuickGraph;
using QuickGraph.Algorithms;

namespace SIL.Cog.GraphAlgorithms
{
	public class BundleEdgeRoutingAlgorithm<TVertex, TEdge, TGraph> : AlgorithmBase<TGraph>, IEdgeRoutingAlgorithm<TVertex, TEdge, TGraph>
		where TVertex : class
		where TEdge : IEdge<TVertex>
		where TGraph : IVertexAndEdgeListGraph<TVertex, TEdge>
	{
		private readonly IDictionary<TVertex, Point> _vertexPositions;
		private readonly IDictionary<TVertex, Size> _vertexSizes;
		private readonly Dictionary<TEdge, Point[]> _edgeRoutes;
		private readonly BundleEdgeRoutingParameters _parameters;

		public BundleEdgeRoutingAlgorithm(TGraph visitedGraph, IDictionary<TVertex, Point> vertexPositions, IDictionary<TVertex, Size> vertexSizes, BundleEdgeRoutingParameters parameters)
			: base(visitedGraph)
		{
			_vertexPositions = vertexPositions;
			_vertexSizes = vertexSizes;
			_parameters = parameters;
			_edgeRoutes = new Dictionary<TEdge, Point[]>();
		}

		public IDictionary<TEdge, Point[]> EdgeRoutes
		{
			get { return _edgeRoutes; }
		}

		protected override void InternalCompute()
		{
			var visibilityGraph = new VisibilityGraph();
			foreach (TVertex vertex in VisitedGraph.Vertices)
			{
				Point pos = _vertexPositions[vertex];
				Size sz = _vertexSizes[vertex];
				var rect = new Rect(new Point(pos.X - (sz.Width / 2), pos.Y - (sz.Height / 2)), sz);
				rect.Inflate(_parameters.VertexMargin, _parameters.VertexMargin);
				visibilityGraph.Obstacles.Add(new Obstacle(Convert(rect.TopLeft), Convert(rect.TopRight), Convert(rect.BottomRight), Convert(rect.BottomLeft)));
			}

			foreach (TEdge edge in VisitedGraph.Edges)
			{
				visibilityGraph.SinglePoints.Add(Convert(_vertexPositions[edge.Source]));
				visibilityGraph.SinglePoints.Add(Convert(_vertexPositions[edge.Target]));
			}

			var vertexPoints = new HashSet<Point2D>(_vertexPositions.Select(kvp => Convert(kvp.Value)));
			
			visibilityGraph.Compute();
			IUndirectedGraph<Point2D, Edge<Point2D>> graph = visibilityGraph.Graph;
			var usedEdges = new HashSet<Edge<Point2D>>();
			foreach (TEdge edge in VisitedGraph.Edges)
			{
				Point2D pos1 = Convert(_vertexPositions[edge.Source]);
				Point2D pos2 = Convert(_vertexPositions[edge.Target]);
				TryFunc<Point2D, IEnumerable<Edge<Point2D>>> paths = graph.ShortestPathsDijkstra(e => GetWeight(vertexPoints, usedEdges, pos1, pos2, e), pos1);
				IEnumerable<Edge<Point2D>> path;
				if (paths(pos2, out path))
				{
					var edgeRoute = new List<Point>();
					bool first = true;
					Point2D point = pos1;
					foreach (Edge<Point2D> e in path)
					{
						if (!first)
							edgeRoute.Add(Convert(point));
						usedEdges.Add(e);
						point = e.GetOtherVertex(point);
						first = false;
					}
					_edgeRoutes[edge] = edgeRoute.ToArray();
				}
			}
		}

		protected virtual double GetWeight(HashSet<Point2D> vertexPoints, HashSet<Edge<Point2D>> usedEdges, Point2D pos1, Point2D pos2, Edge<Point2D> edge)
		{
			if (vertexPoints.Contains(edge.Source) && (!edge.Source.Equals(pos1) && !edge.Source.Equals(pos2)))
				return double.PositiveInfinity;

			if (vertexPoints.Contains(edge.Target) && (!edge.Target.Equals(pos1) && !edge.Target.Equals(pos2)))
				return double.PositiveInfinity;

			double length = Math.Sqrt(Math.Pow(edge.Source.X - edge.Target.X, 2) + Math.Pow(edge.Source.Y - edge.Target.Y, 2));
			double ink = usedEdges.Contains(edge) ? 0 : length;
			return (_parameters.InkCoefficient * ink) + (_parameters.LengthCoefficient * length);
		}

		private static Point2D Convert(Point p)
		{
			return new Point2D(p.X, p.Y);
		}

		private static Point Convert(Point2D p)
		{
			return new Point(p.X, p.Y);
		}
	}
}
