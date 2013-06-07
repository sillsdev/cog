using System;
using System.Collections.Generic;
using SIL.Collections;

namespace SIL.Cog.GraphAlgorithms
{
	internal class RotationTreeNode : OrderedBidirTreeNode<RotationTreeNode>, IComparable<RotationTreeNode>
	{
		private readonly Obstacle _obstacle;
		private readonly Point2D _point;
		private readonly List<ObstacleSegment> _segments;
		private readonly bool _isSinglePoint;
 
		public RotationTreeNode(Obstacle obstacle, Point2D point, bool isSinglePoint)
			: base(b => new RotationTreeNode(new Point2D(0, 0)))
		{
			_obstacle = obstacle;
			_point = point;
			_isSinglePoint = isSinglePoint;
			_segments = new List<ObstacleSegment>();
		}

		public RotationTreeNode(Point2D point)
			: this(null, point, true)
		{
		}

		public Point2D Point
		{
			get { return _point; }
		}

		public double X
		{
			get { return _point.X; }
		}

		public double Y
		{
			get { return _point.Y; }
		}

		public Obstacle Obstacle
		{
			get { return _obstacle; }
		}

		public bool IsSinglePoint
		{
			get { return _isSinglePoint; }
		}

		public IList<ObstacleSegment> Segments
		{
			get { return _segments; }
		}

		public ObstacleSegment VisibleSegment { get; set; }

		public ObstacleSegment GetOtherAdjacentSegment(RotationTreeNode p)
		{
			return _segments[0].Point1 == this || _segments[0].Point2 == this ? _segments[1] : _segments[0];
		}

		public ObstacleSegment GetOtherAdjacentSegment(ObstacleSegment s)
		{
			return _segments[0] == s ? _segments[1] : _segments[0];
		}

		public bool IsAdjacent(RotationTreeNode p)
		{
			if (_segments.Count != 2)
				return false;

			return _segments[0].Contains(p) || _segments[1].Contains(p);
		}

		public int CompareTo(RotationTreeNode other)
		{
			int res = X.CompareTo(other.X);
			if (res != 0)
				return res;

			return Y.CompareTo(other.Y);
		}

		public override string ToString()
		{
			return _point.ToString();
		}
	}
}
