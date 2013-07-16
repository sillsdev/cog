namespace SIL.Cog.Applications.GraphAlgorithms
{
	internal class ObstacleSegment
	{
		private readonly Obstacle _obstacle; 
		private readonly RotationTreeNode _point1;
		private readonly RotationTreeNode _point2;

		public ObstacleSegment(Obstacle obstacle, RotationTreeNode point1, RotationTreeNode point2)
		{
			_obstacle = obstacle;
			_point1 = point1;
			_point1.Segments.Add(this);
			_point2 = point2;
			_point2.Segments.Add(this);
		}

		public RotationTreeNode Point1
		{
			get { return _point1; }
		}

		public RotationTreeNode Point2
		{
			get { return _point2; }
		}

		public Obstacle Obstacle
		{
			get { return _obstacle; }
		}

		public bool Contains(RotationTreeNode p)
		{
			return _point1 == p || _point2 == p;
		}

		public RotationTreeNode GetOtherPoint(RotationTreeNode p)
		{
			return _point1 == p ? _point2 : _point1;
		}
	}
}
