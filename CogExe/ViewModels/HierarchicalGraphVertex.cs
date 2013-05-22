using SIL.Cog.Controls;
using SIL.Cog.GraphAlgorithms;

namespace SIL.Cog.ViewModels
{
	public class HierarchicalGraphVertex : WrapperViewModel, IAngledVertex
	{
		private readonly Variety _variety;
		private readonly double _depth;
		private double _angle;

		public HierarchicalGraphVertex(double depth)
		{
			_depth = depth;
		}

		public HierarchicalGraphVertex(Variety variety, double depth)
			: base(variety)
		{
			_variety = variety;
			_depth = depth;
		}

		public string Name
		{
			get
			{
				if (_variety == null)
					return "";
				return _variety.Name;
			}
		}

		public double Depth
		{
			get { return _depth; }
		}

		public bool IsCluster
		{
			get { return _variety == null; }
		}

		public override string ToString()
		{
			return Name;
		}

		public double Angle
		{
			get { return _angle; }
			set { Set(() => Angle, ref _angle, value); }
		}
	}
}
