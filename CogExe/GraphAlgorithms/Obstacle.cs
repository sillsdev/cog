using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SIL.Collections;

namespace SIL.Cog.GraphAlgorithms
{
	public class Obstacle : IEquatable<Obstacle>
	{
		private readonly ReadOnlyList<RotationTreeNode> _nodes;
		private readonly ReadOnlyList<ObstacleSegment> _segments;
		private readonly ReadOnlyList<Point2D> _points; 

		public Obstacle(params Point2D[] points)
			: this((IEnumerable<Point2D>) points)
		{
		}

		public Obstacle(IEnumerable<Point2D> points)
		{
			_points = new ReadOnlyList<Point2D>(points.ToArray());
			var pts = new List<RotationTreeNode>();
			var segs = new List<ObstacleSegment>();
			RotationTreeNode lastPoint = null;
			foreach (Point2D point in _points)
			{
				var newPoint = new RotationTreeNode(this, point, false);
				pts.Add(newPoint);
				if (lastPoint != null)
					segs.Add(new ObstacleSegment(this, lastPoint, newPoint));

				lastPoint = newPoint;
			}

			if (pts.Count > 2)
				segs.Add(new ObstacleSegment(this, pts[pts.Count - 1], pts[0]));

			_nodes = new ReadOnlyList<RotationTreeNode>(pts);
			_segments = new ReadOnlyList<ObstacleSegment>(segs);
		}

		public IReadOnlyList<Point2D> Points
		{
			get { return _points; }
		}

		internal IReadOnlyList<RotationTreeNode> Nodes
		{
			get { return _nodes; }
		}

		internal IReadOnlyList<ObstacleSegment> Segments
		{
			get { return _segments; }
		}

		public bool Contains(Point2D p)
		{
			if (_points.Count < 3)
				return false;

			bool inside = false;
			for (int i = 0, j = _points.Count - 1; i < _points.Count; j = i++)
			{
				if (((_points[i].Y > p.Y) != (_points[j].Y > p.Y))
				    && (p.X < (_points[j].X - _points[i].X) * (p.Y - _points[i].Y) / (_points[j].Y - _points[i].Y) + _points[i].X))
				{
					inside = !inside;
				}
			}
			return inside;
		}

		public bool Equals(Obstacle other)
		{
			if (_points.Count != other._points.Count)
				return false;

			int index1 = MinIndex;
			int index2 = other.MinIndex;
			for (int i = 0; i < _points.Count; i++)
			{
				if (!_points[(i + index1) % _points.Count].Equals(other._points[(i + index2) % _points.Count]))
					return false;
			}
			return true;
		}

		public override bool Equals(object obj)
		{
			var other = obj as Obstacle;
			return other != null && Equals(other);
		}

		public override int GetHashCode()
		{
			int index = MinIndex;
			int code = 23;
			for (int i = 0; i < _points.Count; i++)
				code = code * 31 + _points[(i + index) % _points.Count].GetHashCode();
			return code;
		}

		private int MinIndex
		{
			get { return _nodes.Select((n, i) => new {Node = n, Index = i}).MinBy(ni => ni.Node).Index; }
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			bool first = true;
			sb.Append("[");
			foreach (Point2D point in _points)
			{
				if (!first)
					sb.Append(",");
				sb.Append(point);
				first = false;
			}
			sb.Append("]");
			return sb.ToString();
		}
	}
}
