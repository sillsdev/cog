using System.Collections.Generic;
using SIL.Cog.SequenceAlignment;
using SIL.Collections;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Components
{
	public class Aline : WordPairAlignerBase
	{
		private readonly AlineScorer _scorer;

		public Aline(IEnumerable<SymbolicFeature> relevantVowelFeatures, IEnumerable<SymbolicFeature> relevantConsFeatures,
			IDictionary<SymbolicFeature, int> featureWeights, IDictionary<FeatureSymbol, int> valueMetrics)
			: this(relevantVowelFeatures, relevantConsFeatures, featureWeights, valueMetrics, new WordPairAlignerSettings())
		{
		}

		public Aline(IEnumerable<SymbolicFeature> relevantVowelFeatures, IEnumerable<SymbolicFeature> relevantConsFeatures,
			IDictionary<SymbolicFeature, int> featureWeights, IDictionary<FeatureSymbol, int> valueMetrics, WordPairAlignerSettings settings)
			: base(settings)
		{
			_scorer = new AlineScorer(relevantVowelFeatures, relevantConsFeatures, featureWeights, valueMetrics, settings.ContextualSoundClasses);
		}

		public IReadOnlySet<SymbolicFeature> RelevantVowelFeatures
		{
			get { return _scorer.RelevantVowelFeatures; }
		}

		public IReadOnlySet<SymbolicFeature> RelevantConsonantFeatures
		{
			get { return _scorer.RelevantConsonantFeatures; }
		} 

		public IReadOnlyDictionary<SymbolicFeature, int> FeatureWeights
		{
			get { return _scorer.FeatureWeights; }
		}

		public IReadOnlyDictionary<FeatureSymbol, int> ValueMetrics
		{
			get { return _scorer.ValueMetrics; }
		}

		protected override IPairwiseAlignmentScorer<Word, ShapeNode> Scorer
		{
			get { return _scorer; }
		}

		public override int Delta(FeatureStruct fs1, FeatureStruct fs2)
		{
			return _scorer.Delta(fs1, fs2);
		}
	}
}
