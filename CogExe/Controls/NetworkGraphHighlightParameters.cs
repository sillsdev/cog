using GraphSharp.Algorithms.Highlight;

namespace SIL.Cog.Controls
{
	public class NetworkGraphHighlightParameters : HighlightParameterBase
	{
		private double _simScoreFilter;

		public double SimilarityScoreFilter
		{
			get { return _simScoreFilter; }
			set
			{
				_simScoreFilter = value;
				OnPropertyChanged("SimilarityScoreFilter");
			}
		}
	}
}
