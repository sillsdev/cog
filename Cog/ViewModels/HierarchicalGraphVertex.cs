namespace SIL.Cog.ViewModels
{
	public class HierarchicalGraphVertex : WrapperViewModel
	{
		private readonly Variety _variety;

		public HierarchicalGraphVertex()
		{
		}

		public HierarchicalGraphVertex(Variety variety)
			: base(variety)
		{
			_variety = variety;
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
