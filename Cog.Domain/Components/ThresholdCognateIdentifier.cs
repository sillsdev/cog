using SIL.Machine.Annotations;
using SIL.Machine.SequenceAlignment;

namespace SIL.Cog.Domain.Components
{
	public class ThresholdCognateIdentifier : IProcessor<VarietyPair>
	{
		private readonly CogProject _project;
		private readonly double _threshold;
		private readonly string _alignerID;

		public ThresholdCognateIdentifier(CogProject project, double threshold, string alignerID)
		{
			_project = project;
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

		public void Process(VarietyPair varietyPair)
		{
			double totalScore = 0.0;
			int totalCognateCount = 0;
			IWordAligner aligner = _project.WordAligners[_alignerID];
			foreach (WordPair wordPair in varietyPair.WordPairs)
			{
				IWordAlignerResult alignerResult = aligner.Compute(wordPair);
				int alignmentCount = 0;
				double totalAlignmentScore = 0.0;
				foreach (Alignment<Word, ShapeNode> alignment in alignerResult.GetAlignments())
				{
					totalAlignmentScore += alignment.NormalizedScore;
					alignmentCount++;
				}
				wordPair.PhoneticSimilarityScore = totalAlignmentScore / alignmentCount;
				wordPair.CognicityScore = wordPair.PhoneticSimilarityScore;
				totalScore += wordPair.PhoneticSimilarityScore;
				wordPair.AreCognatePredicted = wordPair.PhoneticSimilarityScore >= _threshold;
				if (wordPair.AreCognatePredicted)
					totalCognateCount++;
			}

			int wordPairCount = varietyPair.WordPairs.Count;
			if (wordPairCount == 0)
			{
				varietyPair.PhoneticSimilarityScore = 0;
				varietyPair.LexicalSimilarityScore = 0;
			}
			else
			{
				varietyPair.PhoneticSimilarityScore = totalScore / wordPairCount;
				varietyPair.LexicalSimilarityScore = (double) totalCognateCount / wordPairCount;
			}
		}
	}
}
