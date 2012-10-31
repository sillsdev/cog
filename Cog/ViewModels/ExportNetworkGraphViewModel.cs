namespace SIL.Cog.ViewModels
{
	public class ExportNetworkGraphViewModel : CogViewModelBase
	{
		private SimilarityMetric _similarityMetric;

		public ExportNetworkGraphViewModel()
			: base("Export network graph")
		{
		}

		public SimilarityMetric SimilarityMetric
		{
			get { return _similarityMetric; }
			set { Set(() => SimilarityMetric, ref _similarityMetric, value); }
		}
	}
}
