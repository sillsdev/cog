using System;
using System.Collections.Generic;
using System.Linq;
using QuickGraph;
using SIL.Collections;

namespace SIL.Cog.Applications.GraphAlgorithms
{
	public class VisibilityGraph
	{
		private readonly HashSet<Obstacle> _obstacles;
		private readonly List<RotationTreeNode> _nodes;
		private UndirectedGraph<Point2D, Edge<Point2D>> _graph;
		private readonly HashSet<Point2D> _singlePoints;
		private RotationTreeNode _minusInf;
		private RotationTreeNode _plusInf;

		public VisibilityGraph()
			: this(Enumerable.Empty<Obstacle>(), Enumerable.Empty<Point2D>())
		{
		}

		public VisibilityGraph(IEnumerable<Obstacle> obstacles, IEnumerable<Point2D> singlePoints)
		{
			_obstacles = new HashSet<Obstacle>(obstacles);
			_singlePoints = new HashSet<Point2D>(singlePoints);
			_nodes = new List<RotationTreeNode>();
		}

		public ISet<Obstacle> Obstacles
		{
			get { return _obstacles; }
		}

		public ISet<Point2D> SinglePoints
		{
			get { return _singlePoints; }
		}

		public void Compute()
		{
			Initialize();
			InitializeNodeVisibility();
			ComputeRotationTree();
		}

		public IUndirectedGraph<Point2D, Edge<Point2D>> Graph
		{
			get { return _graph; }
		}

		private void Initialize()
		{
			_graph = new UndirectedGraph<Point2D, Edge<Point2D>>();
			_nodes.Clear();
			foreach (Obstacle obstacle in _obstacles)
			{
				foreach (RotationTreeNode node in obstacle.Nodes)
				{
					_nodes.Add(node);
					_graph.AddVertex(node.Point);
				}
				_graph.AddEdgeRange(obstacle.Segments.Select(s => new Edge<Point2D>(s.Point1.Point, s.Point2.Point)));
			}
			foreach (Point2D point in _singlePoints)
			{
				Obstacle obstacle = _obstacles.FirstOrDefault(o => o.Contains(point));
				var newPoint = new RotationTreeNode(obstacle, point, true);
				_graph.AddVertex(point);
				_nodes.Add(newPoint);
			}

			double maxX = _nodes.Max(p => p.Point.X);
			_plusInf = new RotationTreeNode(new Point2D(maxX + 100, double.PositiveInfinity));
			_minusInf = new RotationTreeNode(new Point2D(maxX + 100, double.NegativeInfinity));
			_plusInf.Children.Add(_minusInf);
			_minusInf.Children.AddRange(_nodes.OrderByDescending(n => n));
		}

		private void InitializeNodeVisibility()
		{
			foreach (RotationTreeNode obstaclePoint in _nodes)
			{
				double minDist = double.MaxValue;
				ObstacleSegment minDistSeg = null;
				foreach (ObstacleSegment obstacleSegment in _obstacles.SelectMany(o => o.Segments))
				{
					if (obstacleSegment.Contains(obstaclePoint))
						continue;
					if (obstaclePoint.IsSinglePoint && obstaclePoint.Obstacle == obstacleSegment.Obstacle)
						continue;

					double pointX = obstaclePoint.X;
					double pointY = obstaclePoint.Y;
				
					double segPoint1X = obstacleSegment.Point1.X;
					double segPoint1Y = obstacleSegment.Point1.Y;
					double segPoint2X = obstacleSegment.Point2.X;
					double segPoint2Y = obstacleSegment.Point2.Y;

					double segMinX = Math.Min(segPoint1X, segPoint2X);
					double segMinY = Math.Min(segPoint1Y, segPoint2Y);
					double segMaxX = Math.Max(segPoint1X, segPoint2X);
					double segMaxY = Math.Max(segPoint1Y, segPoint2Y);

					double dist;
					if (segMinX <= pointX && segMaxX > pointX)
					{
						// if segment stretches across the point in x direction

						// We have to calculate the distance from the point
						// straight down to the segment.
						// Because the segment have a slope, we have to do a little math.

						if (Math.Abs(segMinY - segMaxY) < double.Epsilon)
						{
							// special case: the segment is horizontal
							dist = segMinY - pointY;
						}
						else if (Math.Abs(segMinX - segMaxX) < double.Epsilon)
						{
							// special case: the segment is vertical
							dist = Math.Min(segMinY - pointY, segMaxY - pointY);
						}
						else
						{
							// case: angled segment
							double pointOnSegmentX = pointX;
							double pointOnSegmentY;
							if (Math.Abs(pointOnSegmentX - segMinX) > double.Epsilon)
								pointOnSegmentY = segMinY + ((segMaxY - segMinY) / (segMaxX - segMinX) * (pointOnSegmentX - segMinX));
							else
								pointOnSegmentY = segMinY;

							dist = pointOnSegmentY - pointY;
						}
					}
					else
					{
						continue;
					}

					if (dist > 0)
					{	// segment lies above point not below it
						continue;
					}

					dist = Math.Abs(dist);
					if (dist < minDist)
					{	// check if current distance is smaller than previous ones
						minDist = dist;
						minDistSeg = obstacleSegment;
					}
				}

				obstaclePoint.VisibleSegment = minDistSeg;
			}
		}

		private void ComputeRotationTree()
		{
			var stack = new Stack<RotationTreeNode>();
			stack.Push(_minusInf.Children.First);

			while (stack.Count > 0)
			{
				RotationTreeNode p = stack.Pop();

				RotationTreeNode pr = p.Next;
				RotationTreeNode q = p.Parent;

				if (q != _minusInf)
					HandleWithPoints(p, q);

				p.Remove();
				RotationTreeNode z = q.Prev;
				if (z == z.List.Begin || !LeftTurn(p, z, z.Parent))
				{
					q.Prev.AddAfter(p);
				}
				else
				{
					while (!z.IsLeaf && LeftTurn(p, z.Children.Last, z))
						z = z.Children.Last;
					z.Children.Add(p);
					if (stack.Count > 0 && z == stack.Peek())
						stack.Pop();
				}
				if (p.Prev == p.List.Begin && p.Parent != _plusInf)
					stack.Push(p);
				if (pr != pr.List.End)
					stack.Push(pr);
			}
		}

		private static bool LeftTurn(RotationTreeNode p, RotationTreeNode q, RotationTreeNode r)
		{
			if (r == null)
				return false;

			if (double.IsNegativeInfinity(p.Y) || double.IsNegativeInfinity(q.Y) || double.IsNegativeInfinity(r.Y)
			    || double.IsPositiveInfinity(p.Y) || double.IsPositiveInfinity(q.Y))
			{
				return false;
			}

			if (double.IsPositiveInfinity(r.Y))
				return p.X <= q.X || (Math.Abs(p.X - q.X) < double.Epsilon && p.Y > q.Y);

			double m = (q.Y - p.Y) / (q.X - p.X);
			double b = p.Y - (m * p.X);
			double rValue = (m * r.X) + b;
			if (r.Y > rValue)
				return p.X <= q.X;
			if (r.Y < rValue)
				return p.X > q.X;
			return false;
		}

		private void HandleWithPoints(RotationTreeNode p, RotationTreeNode q)
		{
			if (p == null || q == null)
				return;

			if (p.X > q.X)
				return;

			if (p.IsSinglePoint && !q.IsSinglePoint && p.Obstacle == q.Obstacle)
			{
				p.VisibleSegment = q.VisibleSegment;
				//AddEdge(p, q);
			}
			else if (q.IsSinglePoint && !p.IsSinglePoint && q.Obstacle == p.Obstacle)
			{
				//AddEdge(p, q);
			}
			else if (q.IsSinglePoint && p.VisibleSegment != null && q.Obstacle == p.VisibleSegment.Obstacle)
			{
				AddEdge(p, q);
			}
			else if (p.IsAdjacent(q))
			{
				ObstacleSegment higher = GetHigherSegment(q, p);
				p.VisibleSegment = higher ?? q.VisibleSegment;
				if (!SameObstacle(p, q))
					AddEdge(p, q);
			}
			else if (p.VisibleSegment != null && p.VisibleSegment.Contains(q))
			{
				ObstacleSegment left = GetLeftSegment(q, p);
				p.VisibleSegment = left ?? q.VisibleSegment;
				if (!SameObstacle(p, q))
					AddEdge(p, q);
			}
			else if (PointLiesNearerThanSegment(p, q))
			{
				p.VisibleSegment = q.IsSinglePoint ? q.VisibleSegment : GetNearerSegment(q, p);
				if (!SameObstacle(p, q))
					AddEdge(p, q);
			}
		}

		private void AddEdge(RotationTreeNode p, RotationTreeNode q)
		{
			if (_graph.ContainsVertex(p.Point) && _graph.ContainsVertex(q.Point))
				_graph.AddEdge(new Edge<Point2D>(p.Point, q.Point));
		}

		private bool SameObstacle(RotationTreeNode p, RotationTreeNode q)
		{
			return !p.IsSinglePoint && !q.IsSinglePoint && p.Obstacle.Nodes.Contains(q);
		}

		private static ObstacleSegment GetHigherSegment(RotationTreeNode node, RotationTreeNode p)
		{
			foreach (ObstacleSegment seg in node.Segments)
			{
				RotationTreeNode other = seg.GetOtherPoint(node);
				if (other != p && other.Y > node.Y)
					return seg;
			}

			return null;
		}

		private static ObstacleSegment GetNearerSegment(RotationTreeNode node, RotationTreeNode p)
		{
			ObstacleSegment segment1 = node.Segments[0];
			ObstacleSegment segment2 = node.Segments[1];

			return CalcDistance(p.Point, segment1.Point1.Point, segment1.Point2.Point) < CalcDistance(p.Point, segment2.Point1.Point, segment2.Point2.Point) ? segment1 : segment2;
		}

		private static ObstacleSegment GetLeftSegment(RotationTreeNode node, RotationTreeNode p)
		{
			ObstacleSegment seg1 = node.Segments[0];
			ObstacleSegment seg2 = node.Segments[1];
			RotationTreeNode pointSeg1 = seg1.GetOtherPoint(node);
			RotationTreeNode pointSeg2 = seg2.GetOtherPoint(node);
		
			// using a linear equation: y = mx + b
			double m = (node.Y - p.Y) / (node.X - p.X);
			double b = p.Y - (m * p.X);
		
			// there now are several cases:
			// 1) one of the two segments lies to the left
			// 2) none lies to the left
			// 3) both lie to the left
			// 4) one segment lies directly behind the line p to q
		
			// in case
			// 1) we return that segment
			// 2) we return null
			// 3) we return the segment nearer to p
			// 4) we return that segment (if the other does not lie to the left)

			// check the cases if the line p-to-this 
			// is vertical
			if (double.IsInfinity(m))
			{
				if (p.Y > node.Y)
				{	// consider the direction of the vertical line
					// case 1
					if (pointSeg1.X < node.X && node.X <= pointSeg2.X)
						return seg1;	// Segment 1 lies to the left of pq
					if (pointSeg2.X < node.X && node.X <= pointSeg1.X)
						return seg2;	// Segment 2 lies to the left of pq
				
					// case 3
					if (pointSeg1.X < node.X && pointSeg2.X < node.X)
						//return node.GetNearestSegment(p);
						return GetNearerSegment(node, p);
					

					// case 4
					if (Math.Abs(pointSeg1.X - node.X) < double.Epsilon)
						return seg1;
					if (Math.Abs(pointSeg2.X - node.X) < double.Epsilon)
						return seg2;
				
					// case 2
					if (pointSeg1.X > node.X && pointSeg2.X > node.X)
						return null;
				}
				else
				{
					// case 1
					if (pointSeg1.X > node.X && node.X >= pointSeg2.X)
						return seg1;	// Segment 1 lies to the left of pq
					if (pointSeg2.X > node.X && node.X >= pointSeg1.X)
						return seg2;	// Segment 2 lies to the left of pq

					// case 3
					if (pointSeg1.X > node.X && pointSeg2.X > node.X)
						//return node.GetNearestSegment(p);
						return GetNearerSegment(node, p);

					// case 4
					if (Math.Abs(pointSeg1.X - node.X) < double.Epsilon)
						return seg1;
					if (Math.Abs(pointSeg2.X - node.X) < double.Epsilon)
						return seg2;
				
					// case 2
					if (pointSeg1.X < node.X && pointSeg2.X < node.X)
						return null;
				}
			}
			else
			{	// check for non-vertical lines
				// function value for end point of segment 1
				double value1 = (m * pointSeg1.X) + b;
				// function value for end point of segment 2
				double value2 = (m * pointSeg2.X) + b;
			
				// case 1
				if (pointSeg1.Y > value1 && value2 >= pointSeg2.Y)
					return seg1;	// Segment 1 lies to the left of pq
				if (pointSeg2.Y > value2 && value1 >= pointSeg1.Y)
					return seg2;	// Segment 2 lies to the left of pq
			
				// case 3
				if (pointSeg1.Y > value1 && pointSeg2.Y > value2)
					//return node.GetNearestSegment(p);
					return GetNearerSegment(node, p);

				// case 4
				if (Math.Abs(pointSeg1.Y - value1) < double.Epsilon)
					return seg1;
				if (Math.Abs(pointSeg2.Y - value2) < double.Epsilon)
					return seg2;
			
				// case 2
				if (pointSeg1.Y < value1 && pointSeg2.Y < value2)
					return null;
			}

			// return null if this.equals(p)
			return null;
		}

		private static bool PointLiesNearerThanSegment(RotationTreeNode p, RotationTreeNode q)
		{
			ObstacleSegment visSegment = p.VisibleSegment;
			if (visSegment == null)	// if there is no segment, it can not lie nearer (to p) than q
				return true;

			// using a linear equation: y = mx + b
			// for the line pq
			double m1 = (q.Y - p.Y) / (q.X - p.X);
			double b1 = p.Y - (m1 * p.X);

			// getting the points of the segment
			RotationTreeNode point1 = visSegment.Point1;
			RotationTreeNode point2 = visSegment.Point2;

			// check if both segment points lie on the
			// same side of that line.
			// if they do, there is no crossing.
			if (!double.IsInfinity(m1))
			{
			    double valuePoint1 = (m1 * point1.X) + b1;
			    double valuePoint2 = (m1 * point2.X) + b1;
			    if ((point1.Y > valuePoint1 && point2.Y > valuePoint2)
			        || (point1.Y < valuePoint1 && point2.Y < valuePoint2))
			    {
			        return true;
			    }
			}
			else if (point1.X < p.X && point2.X < p.X || point1.X > p.X && point2.X > p.X)
			{
			    return true;
			}

			// get the line for the segment
			double m2 = (point2.Y - point1.Y) / (point2.X - point1.X);
			double b2 = point1.Y - (m2 * point1.X);


			if (Math.Abs(m1 - m2) < double.Epsilon)	// avoiding divide-by-zero; lines are parallel
			    return true;	// if pq and the segment are parallel, the segment can be discarded and the point "lies nearer"

			// calculate the crossing point
			double x;
			if (!double.IsInfinity(m2) && !double.IsInfinity(m1))
			    x = (b2 - b1) / (m1 - m2);
			else if (!double.IsInfinity(m2) && double.IsInfinity(m1))
			    x = p.X;
			else 
			    x = point1.X;

			double y = m1 * x + b1;

			var crossingPoint = new Point2D(x, y);

			// check if distancePQ is the smallest
			return CalcDistance(p.Point, q.Point) <= CalcDistance(p.Point, crossingPoint);
		}

		private static double CalcDistance(Point2D p1, Point2D p2)
		{
			if (Math.Abs(p1.X - p2.X) < double.Epsilon)
				return Math.Abs(p1.Y - p2.Y);
			if (Math.Abs(p1.Y - p2.Y) < double.Epsilon)
				return Math.Abs(p1.X - p2.X);

			return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
		}

		private static double CalcDistance(Point2D p, Point2D v, Point2D w)
		{
			double l2 = Math.Pow(v.X - w.X, 2) + Math.Pow(v.Y - w.Y, 2);
			if (Math.Abs(l2) < double.Epsilon)
				return CalcDistance(p, v);
			double t = ((p.X - v.X) * (w.X - v.X) + (p.Y - v.Y) * (w.Y - v.Y)) / l2;
			if (t < 0)
				return CalcDistance(p, v);
			if (t > 1)
				return CalcDistance(p, w);
			var proj = new Point2D(v.X + t * (w.X - v.X), v.Y + t * (w.Y - v.Y));
			return CalcDistance(p, proj);
		}
	}
}
