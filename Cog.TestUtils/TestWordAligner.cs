using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;
using SIL.Machine.Annotations;
using SIL.Machine.FeatureModel;
using SIL.Machine.SequenceAlignment;

namespace SIL.Cog.TestUtils
{
	public class TestWordAligner : WordAlignerBase
	{
		private readonly TestScorer _scorer;

		public TestWordAligner(SegmentPool segmentPool)
			: base(new WordPairAlignerSettings())
		{
			_scorer = new TestScorer(segmentPool);
		}

		protected override IPairwiseAlignmentScorer<Word, ShapeNode> Scorer
		{
			get { return _scorer; }
		}

		public override int Delta(FeatureStruct fs1, FeatureStruct fs2)
		{
			return fs1.ValueEquals(fs2) ? 0 : 100;
		}

		private class TestScorer : IPairwiseAlignmentScorer<Word, ShapeNode>
		{
			private readonly SegmentPool _segmentPool;

			public TestScorer(SegmentPool segmentPool)
			{
				_segmentPool = segmentPool;
			}

			public int GetGapPenalty(Word sequence1, Word sequence2)
			{
				return -100;
			}

			public int GetInsertionScore(Word sequence1, ShapeNode p, Word sequence2, ShapeNode q)
			{
				return 0;
			}

			public int GetDeletionScore(Word sequence1, ShapeNode p, Word sequence2, ShapeNode q)
			{
				return 0;
			}

			public int GetSubstitutionScore(Word sequence1, ShapeNode p, Word sequence2, ShapeNode q)
			{
				return _segmentPool.Get(p).Equals(_segmentPool.Get(q)) ? 100 : 0;
			}

			public int GetExpansionScore(Word sequence1, ShapeNode p, Word sequence2, ShapeNode q1, ShapeNode q2)
			{
				Segment pSeg = _segmentPool.Get(p);
				Segment q1Seg = _segmentPool.Get(q1);
				Segment q2Seg = _segmentPool.Get(q2);

				int score = 0;
				if (pSeg.Equals(q1Seg))
					score += 100;
				if (pSeg.Equals(q2Seg))
					score += 100;
				return score;
			}

			public int GetCompressionScore(Word sequence1, ShapeNode p1, ShapeNode p2, Word sequence2, ShapeNode q)
			{
				Segment qSeg = _segmentPool.Get(q);
				Segment p1Seg = _segmentPool.Get(p1);
				Segment p2Seg = _segmentPool.Get(p2);

				int score = 0;
				if (qSeg.Equals(p1Seg))
					score += 100;
				if (qSeg.Equals(p2Seg))
					score += 100;
				return score;
			}

			public int GetMaxScore1(Word sequence1, ShapeNode p, Word sequence2)
			{
				return 100;
			}

			public int GetMaxScore2(Word sequence1, Word sequence2, ShapeNode q)
			{
				return 100;
			}
		}
	}
}
