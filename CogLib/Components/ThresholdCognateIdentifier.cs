using SIL.Machine;

namespace SIL.Cog.Components
{
	public class ThresholdCognateIdentifier : ProcessorBase<VarietyPair>
	{
		private readonly double _threshold;
		private readonly string _alignerID;

		public ThresholdCognateIdentifier(CogProject project, double threshold, string alignerID)
			: base(project)
		{
			_threshold = threshold;
			_alignerID = alignerID;
		}

		public double Threshold
		{
			get { return _threshold; }
		}

		public string AlignerID
		{
			get { return _alignerID; }
		}

		public override void Process(VarietyPair varietyPair)
		{
			double totalScore = 0.0;
			int totalCognateCount = 0;
			IWordPairAligner aligner = Project.Aligners[_alignerID];
			foreach (WordPair wordPair in varietyPair.WordPairs)
			{
				IWordPairAlignerResult alignerResult = aligner.Compute(wordPair);
				int alignmentCount = 0;
				double totalAlignmentScore = 0.0;
				foreach (Alignment<ShapeNode> alignment in alignerResult.GetAlignments())
				{
					totalAlignmentScore += alignment.NormalizedScore;
					alignmentCount++;
				}
				wordPair.PhoneticSimilarityScore = totalAlignmentScore / alignmentCount;
				totalScore += wordPair.PhoneticSimilarityScore;
				wordPair.AreCognatePredicted = wordPair.PhoneticSimilarityScore >= _threshold;
				if (wordPair.AreCognatePredicted)
					totalCognateCount++;
			}

			int wordPairCount = varietyPair.WordPairs.Count;
			varietyPair.PhoneticSimilarityScore = totalScore / wordPairCount;
			varietyPair.LexicalSimilarityScore = (double) totalCognateCount / wordPairCount;
		}
	}
}
