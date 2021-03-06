using SIL.Cog.Domain;

namespace SIL.Cog.Application.ViewModels
{
	public class HierarchicalGraphVertex : WrapperViewModel
	{
		private readonly Variety _variety;
		private readonly double _depth;

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
	}
}
