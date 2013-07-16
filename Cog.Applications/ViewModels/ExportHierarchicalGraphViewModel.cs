using GalaSoft.MvvmLight;

namespace SIL.Cog.Applications.ViewModels
{
	public class ExportHierarchicalGraphViewModel : ViewModelBase
	{
		private HierarchicalGraphType _graphType;
		private ClusteringMethod _clusteringMethod;
		private SimilarityMetric _similarityMetric;

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
