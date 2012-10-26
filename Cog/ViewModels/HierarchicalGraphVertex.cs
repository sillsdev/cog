namespace SIL.Cog.ViewModels
{
	public class HierarchicalGraphVertex : WrapperViewModel
	{
		private readonly Variety _variety;
		private readonly double _similarityScore;

		public HierarchicalGraphVertex(double similarityScore)
		{
			_similarityScore = similarityScore;
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

		public double SimilarityScore
		{
			get { return _similarityScore; }
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
