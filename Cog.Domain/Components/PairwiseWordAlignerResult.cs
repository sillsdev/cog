using System.Collections.Generic;
using SIL.Machine.Annotations;
using SIL.Machine.SequenceAlignment;
using SIL.ObjectModel;

namespace SIL.Cog.Domain.Components
{
	internal class PairwiseWordAlignerResult : WordAlignerResultBase
	{
		private readonly PairwiseAlignmentAlgorithm<Word, ShapeNode> _algorithm;
		private readonly ReadOnlyList<Word> _words; 

		public PairwiseWordAlignerResult(IWordAligner wordAligner, IPairwiseAlignmentScorer<Word, ShapeNode> scorer, WordPairAlignerSettings settings, Word word1, Word word2)
			: base(wordAligner)
		{
			_words = new ReadOnlyList<Word>(new [] {word1, word2});
			_algorithm = new PairwiseAlignmentAlgorithm<Word, ShapeNode>(scorer, word1, word2, GetNodes)
				{
					ExpansionCompressionEnabled = settings.ExpansionCompressionEnabled,
					Mode = settings.Mode
				};
			_algorithm.Compute();
		}

		public override ReadOnlyList<Word> Words
		{
			get { return _words; }
		}

		public override IEnumerable<Alignment<Word, ShapeNode>> GetAlignments()
		{
			return _algorithm.GetAlignments();
		}

		public override int BestRawScore
		{
			get { return _algorithm.BestRawScore; }
		}
	}
}
