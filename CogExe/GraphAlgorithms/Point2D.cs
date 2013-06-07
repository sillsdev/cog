using System;

namespace SIL.Cog.GraphAlgorithms
{
	public class Point2D : IEquatable<Point2D>
	{
		private readonly double _x;
		private readonly double _y;

		public Point2D(double x, double y)
		{
			_x = x;
			_y = y;
		}

		public double X
		{
			get { return _x; }
		}

		public double Y
		{
			get { return _y; }
		}

		public override bool Equals(object obj)
		{
			var otherPoint = obj as Point2D;
			return otherPoint != null && Equals(otherPoint);
		}

		public bool Equals(Point2D other)
		{
			return other != null && Math.Abs(_x - other._x) < double.Epsilon && Math.Abs(_y - other._y) < double.Epsilon;
		}

		public override int GetHashCode()
		{
			int code = 23;
			code = code * 31 + _x.GetHashCode();
			code = code * 31 + _y.GetHashCode();
			return code;
		}

		public override string ToString()
		{
			return string.Format("({0},{1})", _x, _y);
		}
	}
}
