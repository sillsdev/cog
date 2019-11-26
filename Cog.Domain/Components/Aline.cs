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

		public Aline(SegmentPool segmentPool, IEnumerable<SymbolicFeature> relevantVowelFeatures,
			IEnumerable<SymbolicFeature> relevantConsFeatures, IDictionary<SymbolicFeature, int> featureWeights,
			IDictionary<FeatureSymbol, int> valueMetrics)
			: this(segmentPool, relevantVowelFeatures, relevantConsFeatures, featureWeights, valueMetrics,
				  new AlineSettings())
		{
		}

		public Aline(SegmentPool segmentPool, IEnumerable<SymbolicFeature> relevantVowelFeatures,
			IEnumerable<SymbolicFeature> relevantConsFeatures, IDictionary<SymbolicFeature, int> featureWeights,
			IDictionary<FeatureSymbol, int> valueMetrics, AlineSettings settings)
			: base(settings)
		{
			Settings = settings;
			_scorer = new AlineScorer(segmentPool, relevantVowelFeatures, relevantConsFeatures, featureWeights,
				valueMetrics, settings.ContextualSoundClasses, settings.SoundChangeScoringEnabled,
				settings.SyllablePositionCostEnabled);
		}

		public int MaxIndelScore
		{
			get => _scorer.MaxIndelScore;
			set => _scorer.MaxIndelScore = value;
		}

		public int MaxSoundChangeScore
		{
			get => _scorer.MaxSoundChangeScore;
			set => _scorer.MaxSoundChangeScore = value;
		}

		public int MaxSubstitutionScore
		{
			get => _scorer.MaxSubstitutionScore;
			set => _scorer.MaxSubstitutionScore = value;
		}

		public int MaxExpansionCompressionScore
		{
			get => _scorer.MaxExpansionCompressionScore;
			set => _scorer.MaxExpansionCompressionScore = value;
		}

		public int IndelCost
		{
			get => _scorer.IndelCost;
			set => _scorer.IndelCost = value;
		}

		public int VowelCost
		{
			get => _scorer.VowelCost;
			set => _scorer.VowelCost = value;
		}

		public int SyllablePositionCost
		{
			get => _scorer.SyllablePositionCost;
			set => _scorer.SyllablePositionCost = value;
		}

		public ReadOnlySet<SymbolicFeature> RelevantVowelFeatures => _scorer.RelevantVowelFeatures;

		public ReadOnlySet<SymbolicFeature> RelevantConsonantFeatures => _scorer.RelevantConsonantFeatures;

		public ReadOnlyDictionary<SymbolicFeature, int> FeatureWeights => _scorer.FeatureWeights;

		public ReadOnlyDictionary<FeatureSymbol, int> ValueMetrics => _scorer.ValueMetrics;

		public AlineSettings Settings { get; }

		protected override IPairwiseAlignmentScorer<Word, ShapeNode> Scorer => _scorer;

		public override int Delta(FeatureStruct fs1, FeatureStruct fs2)
		{
			return _scorer.Delta(fs1, fs2);
		}
	}
}
