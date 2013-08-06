using System.Collections.Generic;
using SIL.Cog.Domain.SequenceAlignment;
using SIL.Collections;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Domain.Components
{
	public class Aline : WordAlignerBase
	{
		private readonly AlineScorer _scorer;

		public Aline(SegmentPool segmentPool, IEnumerable<SymbolicFeature> relevantVowelFeatures, IEnumerable<SymbolicFeature> relevantConsFeatures,
			IDictionary<SymbolicFeature, int> featureWeights, IDictionary<FeatureSymbol, int> valueMetrics)
			: this(segmentPool, relevantVowelFeatures, relevantConsFeatures, featureWeights, valueMetrics, new WordPairAlignerSettings())
		{
		}

		public Aline(SegmentPool segmentPool, IEnumerable<SymbolicFeature> relevantVowelFeatures, IEnumerable<SymbolicFeature> relevantConsFeatures,
			IDictionary<SymbolicFeature, int> featureWeights, IDictionary<FeatureSymbol, int> valueMetrics, WordPairAlignerSettings settings)
			: base(settings)
		{
			_scorer = new AlineScorer(segmentPool, relevantVowelFeatures, relevantConsFeatures, featureWeights, valueMetrics, settings.ContextualSoundClasses);
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
