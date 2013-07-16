using System.Collections.Generic;
using SIL.Cog.Domain.SequenceAlignment;
using SIL.Machine;

namespace SIL.Cog.Domain.Components
{
	internal class PairwiseWordAlignerResult : WordAlignerResultBase
	{
		private readonly PairwiseAlignmentAlgorithm<Word, ShapeNode> _algorithm;

		public PairwiseWordAlignerResult(IPairwiseAlignmentScorer<Word, ShapeNode> scorer, WordPairAlignerSettings settings, Word word1, Word word2)
		{
			_algorithm = new PairwiseAlignmentAlgorithm<Word, ShapeNode>(scorer, word1, word2, GetNodes)
				{
					ExpansionCompressionEnabled = settings.ExpansionCompressionEnabled,
					Mode = settings.Mode
				};
			_algorithm.Compute();
		}

		public override IEnumerable<Alignment<Word, ShapeNode>> GetAlignments()
		{
			return _algorithm.GetAlignments();
		}
	}
}
