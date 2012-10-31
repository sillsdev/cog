namespace SIL.Cog.ViewModels
{
	public class ExportSimilarityMatrixViewModel : CogViewModelBase
	{
		private SimilarityMetric _similarityMetric;

		public ExportSimilarityMatrixViewModel()
			: base("Export similarity matrix")
		{
		}

		public SimilarityMetric SimilarityMetric
		{
			get { return _similarityMetric; }
			set { Set(() => SimilarityMetric, ref _similarityMetric, value); }
		}
	}
}
