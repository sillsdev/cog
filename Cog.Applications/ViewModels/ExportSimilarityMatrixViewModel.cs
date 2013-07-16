using GalaSoft.MvvmLight;

namespace SIL.Cog.Applications.ViewModels
{
	public class ExportSimilarityMatrixViewModel : ViewModelBase
	{
		private SimilarityMetric _similarityMetric;

		public SimilarityMetric SimilarityMetric
		{
			get { return _similarityMetric; }
			set { Set(() => SimilarityMetric, ref _similarityMetric, value); }
		}
	}
}
