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
			wordPair.CognacyScore = alignerResult.GetAlignments().First().NormalizedScore;
			wordPair.AreCognatePredicted = wordPair.CognacyScore >= _threshold;
		}
	}
}
