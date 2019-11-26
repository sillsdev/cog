using System.Collections.Generic;
using System.Linq;
using SIL.Machine.Annotations;
using SIL.Machine.SequenceAlignment;
using SIL.ObjectModel;

namespace SIL.Cog.Domain.Components
{
	internal class MultipleWordAlignerResult : WordAlignerResultBase
	{
		private readonly MultipleAlignmentAlgorithm<Word, ShapeNode> _algorithm;
		private readonly ReadOnlyList<Word> _words; 

		public MultipleWordAlignerResult(IWordAligner wordAligner, IPairwiseAlignmentScorer<Word, ShapeNode> scorer, IEnumerable<Word> words)
			: base(wordAligner)
		{
			_words = new ReadOnlyList<Word>(words.ToArray());
			_algorithm = new MultipleAlignmentAlgorithm<Word, ShapeNode>(scorer, _words, GetNodes);
			_algorithm.Compute();
		}

		public override ReadOnlyList<Word> Words
		{
			get { return _words; }
		}

		public override IEnumerable<Alignment<Word, ShapeNode>> GetAlignments()
		{
			yield return _algorithm.GetAlignment();
		}

		public override int BestRawScore => 0;
	}
}
