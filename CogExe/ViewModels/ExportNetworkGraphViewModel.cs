using GalaSoft.MvvmLight;

namespace SIL.Cog.ViewModels
{
	public class ExportNetworkGraphViewModel : ViewModelBase
	{
		private SimilarityMetric _similarityMetric;
		private double _similarityScoreFilter;

		public ExportNetworkGraphViewModel()
		{
			_similarityScoreFilter = 0.7;
		}

		public SimilarityMetric SimilarityMetric
		{
			get { return _similarityMetric; }
			set { Set(() => SimilarityMetric, ref _similarityMetric, value); }
		}

		public double SimilarityScoreFilter
		{
			get { return _similarityScoreFilter; }
			set { Set(() => SimilarityScoreFilter, ref _similarityScoreFilter, value); }
		}
	}
}
