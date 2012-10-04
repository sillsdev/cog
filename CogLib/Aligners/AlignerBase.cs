using System.Collections.Generic;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Aligners
{
	public abstract class AlignerBase : IAligner
	{
		private readonly SpanFactory<ShapeNode> _spanFactory;
		private readonly AlignerSettings _settings;
		private readonly List<NaturalClass> _naturalClasses; 

		protected AlignerBase(SpanFactory<ShapeNode> spanFactory, AlignerSettings settings)
		{
			_spanFactory = spanFactory;
			_settings = settings;
			_naturalClasses = _settings.NaturalClasses == null ? new List<NaturalClass>() : new List<NaturalClass>(_settings.NaturalClasses);
			_settings.ReadOnly = true;
		}

		public IEnumerable<NaturalClass> NaturalClasses
		{
			get { return _naturalClasses; }
		}

		public IAlignerResult Compute(VarietyPair varietyPair, Word word1, Word word2)
		{
			return new AlignerResult(this, varietyPair, word1, word2);
		}

		public IAlignerResult Compute(WordPair wordPair)
		{
			return new AlignerResult(this, wordPair.VarietyPair, wordPair.Word1, wordPair.Word2);
		}

		public SpanFactory<ShapeNode> SpanFactory
		{
			get { return _spanFactory; }
		}

		public AlignerSettings Settings
		{
			get { return _settings; }
		}

		public abstract int SigmaInsertion(VarietyPair varietyPair, ShapeNode q);

		public abstract int SigmaDeletion(VarietyPair varietyPair, ShapeNode p);

		public abstract int SigmaSubstitution(VarietyPair varietyPair, ShapeNode p, ShapeNode q);

		public abstract int SigmaExpansion(VarietyPair varietyPair, ShapeNode p, ShapeNode q1, ShapeNode q2);

		public abstract int SigmaCompression(VarietyPair varietyPair, ShapeNode p1, ShapeNode p2, ShapeNode q);

		public abstract int Delta(FeatureStruct fs1, FeatureStruct fs2);

		public abstract int GetMaxScore1(VarietyPair varietyPair, ShapeNode p);

		public abstract int GetMaxScore2(VarietyPair varietyPair, ShapeNode q);
	}
}
