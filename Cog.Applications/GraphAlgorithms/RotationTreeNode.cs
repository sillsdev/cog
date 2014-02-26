using System;
using System.Collections.Generic;

namespace SIL.Cog.Applications.GraphAlgorithms
{
	internal class RotationTreeNode : IComparable<RotationTreeNode>
	{
		private readonly Obstacle _obstacle;
		private readonly Point2D _point;
		private readonly List<ObstacleSegment> _segments;
		private readonly bool _isSinglePoint;
 
		public RotationTreeNode(Obstacle obstacle, Point2D point, bool isSinglePoint)
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

	    public bool IsLeaf
	    {
	        get { return FirstChild == null; }
	    }

		public RotationTreeNode Next { get; private set; }
		public RotationTreeNode Prev { get; private set; }
        public RotationTreeNode Parent { get; private set; }

        public RotationTreeNode FirstChild { get; private set; }
        public RotationTreeNode LastChild { get; private set; }

        public void AddChild(RotationTreeNode node)
        {
            if (IsLeaf)
            {
                FirstChild = node;
                LastChild = node;
                node.Parent = this;
            }
            else
            {
                LastChild.AddAfter(node);
            }
        }

        public void AddAfter(RotationTreeNode node)
        {
			node.Next = Next;
			Next = node;
			node.Prev = this;
            if (node.Next != null)
			    node.Next.Prev = node;
            if (Parent.LastChild == this)
                Parent.LastChild = node;
            node.Parent = Parent;
        }

        public void AddBefore(RotationTreeNode node)
        {
			node.Prev = Prev;
			Prev = node;
			node.Next = this;
            if (node.Prev != null)
			    node.Prev.Next = node;
            if (Parent.FirstChild == this)
                Parent.FirstChild = node;
            node.Parent = Parent;
        }

        public void Remove()
        {
            if (Parent.FirstChild == this)
                Parent.FirstChild = Next;
            if (Parent.LastChild == this)
                Parent.LastChild = Prev;
            if (Prev != null)
			    Prev.Next = Next;
            if (Next != null)
			    Next.Prev = Prev;
            Next = null;
            Prev = null;
            Parent = null;
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
