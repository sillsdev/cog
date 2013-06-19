using System.Collections.Generic;
using SIL.Cog.SequenceAlignment;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Components
{
	public abstract class WordPairAlignerBase : IWordPairAligner
	{
		private readonly SpanFactory<ShapeNode> _spanFactory;
		private readonly WordPairAlignerSettings _settings;

		protected WordPairAlignerBase(SpanFactory<ShapeNode> spanFactory, WordPairAlignerSettings settings)
		{
			_spanFactory = spanFactory;
			_settings = settings;
			_settings.ReadOnly = true;
		}

		public IEnumerable<SoundClass> ContextualSoundClasses
		{
			get { return _settings.ContextualSoundClasses; }
		}

		public bool ExpansionCompressionEnabled
		{
			get { return _settings.ExpansionCompressionEnabled; }
		}

		public IWordPairAlignerResult Compute(VarietyPair varietyPair, Word word1, Word word2)
		{
			return new WordPairAlignerResult(_settings, new Scorer(this, varietyPair), word1, word2);
		}

		public IWordPairAlignerResult Compute(WordPair wordPair)
		{
			return new WordPairAlignerResult(_settings, new Scorer(this, wordPair.VarietyPair), wordPair.Word1, wordPair.Word2);
		}

		public SpanFactory<ShapeNode> SpanFactory
		{
			get { return _spanFactory; }
		}

		public WordPairAlignerSettings Settings
		{
			get { return _settings; }
		}

		public abstract int Delta(FeatureStruct fs1, FeatureStruct fs2);

		protected abstract int GetInsertionScore(VarietyPair varietyPair, ShapeNode p, ShapeNode q);

		protected abstract int GetDeletionScore(VarietyPair varietyPair, ShapeNode p, ShapeNode q);

		protected abstract int GetSubstitutionScore(VarietyPair varietyPair, ShapeNode p, ShapeNode q);

		protected abstract int GetExpansionScore(VarietyPair varietyPair, ShapeNode p, ShapeNode q1, ShapeNode q2);

		protected abstract int GetCompressionScore(VarietyPair varietyPair, ShapeNode p1, ShapeNode p2, ShapeNode q);

		protected abstract int GetMaxScore1(VarietyPair varietyPair, ShapeNode p);

		protected abstract int GetMaxScore2(VarietyPair varietyPair, ShapeNode q);

		private class Scorer : IPairwiseAlignmentScorer<ShapeNode>
		{
			private readonly WordPairAlignerBase _aligner;
			private readonly VarietyPair _varietyPair;

			public Scorer(WordPairAlignerBase aligner, VarietyPair varietyPair)
			{
				_aligner = aligner;
				_varietyPair = varietyPair;
			}

			public int GetInsertionScore(ShapeNode p, ShapeNode q)
			{
				return _aligner.GetInsertionScore(_varietyPair, p, q);
			}

			public int GetDeletionScore(ShapeNode p, ShapeNode q)
			{
				return _aligner.GetDeletionScore(_varietyPair, p, q);
			}

			public int GetSubstitutionScore(ShapeNode p, ShapeNode q)
			{
				return _aligner.GetSubstitutionScore(_varietyPair, p, q);
			}

			public int GetExpansionScore(ShapeNode p, ShapeNode q1, ShapeNode q2)
			{
				return _aligner.GetExpansionScore(_varietyPair, p, q1, q2);
			}

			public int GetCompressionScore(ShapeNode p1, ShapeNode p2, ShapeNode q)
			{
				return _aligner.GetCompressionScore(_varietyPair, p1, p2, q);
			}

			public int GetMaxScore1(ShapeNode p)
			{
				return _aligner.GetMaxScore1(_varietyPair, p);
			}

			public int GetMaxScore2(ShapeNode q)
			{
				return _aligner.GetMaxScore2(_varietyPair, q);
			}
		}
	}
}
