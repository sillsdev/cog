using System.Linq;

namespace SIL.Cog.Domain.Components
{
	public class ThresholdCognateIdentifier : ICognateIdentifier
	{
		private readonly double _threshold;

		public ThresholdCognateIdentifier(double threshold)
		{
			_threshold = threshold;
		}

		public double Threshold
		{
			get { return _threshold; }
		}

		public void UpdateCognacy(WordPair wordPair, IWordAlignerResult alignerResult)
		{
			wordPair.PredictedCognacyScore = alignerResult.GetAlignments().First().NormalizedScore;
			wordPair.PredictedCognacy = wordPair.PredictedCognacyScore >= _threshold;
		}
	}
}
