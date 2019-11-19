using System.Collections.Generic;
using SIL.Machine.Annotations;
using SIL.Machine.FeatureModel;
using SIL.Machine.SequenceAlignment;
using SIL.ObjectModel;

namespace SIL.Cog.Domain.Components
{
	public class Aline : WordAlignerBase
	{
		private readonly AlineScorer _scorer;
		private readonly AlineSettings _settings;

		public Aline(SegmentPool segmentPool, IEnumerable<SymbolicFeature> relevantVowelFeatures, IEnumerable<SymbolicFeature> relevantConsFeatures,
			IDictionary<SymbolicFeature, int> featureWeights, IDictionary<FeatureSymbol, int> valueMetrics)
			: this(segmentPool, relevantVowelFeatures, relevantConsFeatures, featureWeights, valueMetrics, new AlineSettings())
		{
		}

		public Aline(SegmentPool segmentPool, IEnumerable<SymbolicFeature> relevantVowelFeatures, IEnumerable<SymbolicFeature> relevantConsFeatures,
			IDictionary<SymbolicFeature, int> featureWeights, IDictionary<FeatureSymbol, int> valueMetrics, AlineSettings settings)
			: base(settings)
		{
			_settings = settings;
			_scorer = new AlineScorer(segmentPool, relevantVowelFeatures, relevantConsFeatures, featureWeights, valueMetrics,
				settings.ContextualSoundClasses, settings.SoundChangeScoringEnabled, settings.SyllablePositionCostEnabled);
		}

		public ReadOnlySet<SymbolicFeature> RelevantVowelFeatures
		{
			get { return _scorer.RelevantVowelFeatures; }
		}

		public ReadOnlySet<SymbolicFeature> RelevantConsonantFeatures
		{
			get { return _scorer.RelevantConsonantFeatures; }
		} 

		public ReadOnlyDictionary<SymbolicFeature, int> FeatureWeights
		{
			get { return _scorer.FeatureWeights; }
		}

		public ReadOnlyDictionary<FeatureSymbol, int> ValueMetrics
		{
			get { return _scorer.ValueMetrics; }
		}

		public AlineSettings Settings
		{
			get { return _settings; }
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
