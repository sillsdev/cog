using System.Collections.Generic;
using SIL.Cog.SequenceAlignment;
using SIL.Machine;

namespace SIL.Cog.Components
{
	internal class MultipleWordAlignerResult : WordAlignerResultBase
	{
		private readonly MultipleAlignmentAlgorithm<Word, ShapeNode> _algorithm; 

		public MultipleWordAlignerResult(IPairwiseAlignmentScorer<Word, ShapeNode> scorer, IEnumerable<Word> words)
		{
			_algorithm = new MultipleAlignmentAlgorithm<Word, ShapeNode>(scorer, words, GetNodes);
			_algorithm.Compute();
		}

		public override IEnumerable<Alignment<Word, ShapeNode>> GetAlignments()
		{
			yield return _algorithm.GetAlignment();
		}
	}
}
