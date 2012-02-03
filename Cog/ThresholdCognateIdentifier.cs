namespace SIL.Cog
{
	public class ThresholdCognateIdentifier : IProcessor<VarietyPair>
	{
		private readonly EditDistance _editDistance;
		private readonly double _threshold;

		public ThresholdCognateIdentifier(EditDistance editDistance, double threshold)
		{
			_editDistance = editDistance;
			_threshold = threshold;
		}

		public void Process(VarietyPair varietyPair)
		{
			double totalScore = 0.0;
			int totalCognateCount = 0;
			foreach (WordPair wordPair in varietyPair.WordPairs)
			{
				EditDistanceMatrix editDistanceMatrix = _editDistance.Compute(wordPair);
				int alignmentCount = 0;
				double totalAlignmentScore = 0.0;
				foreach (Alignment alignment in editDistanceMatrix.GetAlignments())
				{
					totalAlignmentScore += alignment.Score;
					alignmentCount++;
				}
				wordPair.PhoneticSimilarityScore = totalAlignmentScore / alignmentCount;
				totalScore += wordPair.PhoneticSimilarityScore;
				wordPair.AreCognatesPredicted = wordPair.PhoneticSimilarityScore >= _threshold;
				if (wordPair.AreCognatesPredicted)
					totalCognateCount++;
			}

			int wordPairCount = varietyPair.WordPairs.Count;
			varietyPair.PhoneticSimilarityScore = totalScore / wordPairCount;
			varietyPair.LexicalSimilarityScore = (double) totalCognateCount / wordPairCount;
		}
	}
}
