namespace SIL.Cog.ViewModels
{
	public class ExportHierarchicalGraphViewModel : CogViewModelBase
	{
		private HierarchicalGraphType _graphType;
		private ClusteringMethod _clusteringMethod;
		private SimilarityMetric _similarityMetric;

		public ExportHierarchicalGraphViewModel()
			: base("Export hierarchical graph")
		{
		}

		public HierarchicalGraphType GraphType
		{
			get { return _graphType; }
			set { Set(() => GraphType, ref _graphType, value); }
		}

		public ClusteringMethod ClusteringMethod
		{
			get { return _clusteringMethod; }
			set { Set(() => ClusteringMethod, ref _clusteringMethod, value); }
		}

		public SimilarityMetric SimilarityMetric
		{
			get { return _similarityMetric; }
			set { Set(() => SimilarityMetric, ref _similarityMetric, value); }
		}
	}
}
