using System.Collections.Generic;
using SIL.Collections;
using SIL.Machine.FeatureModel;
using SIL.Machine.NgramModeling;
using SIL.Machine.Statistics;

namespace SIL.Cog.Domain
{
	public class VarietyPair : ObservableObject
	{
		private IConditionalProbabilityDistribution<SoundContext, Ngram<Segment>> _cognateSoundCorrespondenceProbabilityDistribution;
		private ConditionalFrequencyDistribution<SoundContext, Ngram<Segment>> _cognateSoundCorrespondenceFrequencyDistribution;
		private double _defaultSoundCorrespondenceProb;
		private double _phoneticSimilarityScore;
		private double _lexicalSimilarityScore;
		private double _significance;
		private double _precision;
		private double _recall;
		private int _cognateCount;

		public VarietyPair(Variety variety1, Variety variety2)
		{
			Variety1 = variety1;
			Variety2 = variety2;
			WordPairs = new WordPairCollection(this);

			CognateSoundCorrespondencesByPosition = new ReadOnlyDictionary<FeatureSymbol, SoundCorrespondenceCollection>(
				new Dictionary<FeatureSymbol, SoundCorrespondenceCollection>
				{
					{CogFeatureSystem.Onset, new SoundCorrespondenceCollection()},
					{CogFeatureSystem.Nucleus, new SoundCorrespondenceCollection()},
					{CogFeatureSystem.Coda, new SoundCorrespondenceCollection()}
				});
		}

		public Variety Variety1 { get; }
		public Variety Variety2 { get; }
		public WordPairCollection WordPairs { get; }

		public Variety GetOtherVariety(Variety variety)
		{
			Variety otherVariety = null;
			if (Variety1 == variety)
				otherVariety = Variety2;
			else if (Variety2 == variety)
				otherVariety = Variety1;
			return otherVariety;
		}

		public double PhoneticSimilarityScore
		{
			get { return _phoneticSimilarityScore; }
			set { Set(nameof(PhoneticSimilarityScore), ref _phoneticSimilarityScore, value); }
		}

		public double LexicalSimilarityScore
		{
			get { return _lexicalSimilarityScore; }
			set { Set(nameof(LexicalSimilarityScore), ref _lexicalSimilarityScore, value); }
		}

		public double Significance
		{
			get { return _significance; }
			set { Set(nameof(Significance), ref _significance, value); }
		}

		public double Precision
		{
			get { return _precision; }
			set { Set(nameof(Precision), ref _precision, value); }
		}

		public double Recall
		{
			get { return _recall; }
			set { Set(nameof(Recall), ref _recall, value); }
		}

		public ReadOnlyDictionary<FeatureSymbol, SoundCorrespondenceCollection> CognateSoundCorrespondencesByPosition { get; }

		public ConditionalFrequencyDistribution<SoundContext, Ngram<Segment>> CognateSoundCorrespondenceFrequencyDistribution
		{
			get { return _cognateSoundCorrespondenceFrequencyDistribution; }
			set
			{
				Set(nameof(CognateSoundCorrespondenceFrequencyDistribution), ref _cognateSoundCorrespondenceFrequencyDistribution, value);
			}
		}

		public IConditionalProbabilityDistribution<SoundContext, Ngram<Segment>> CognateSoundCorrespondenceProbabilityDistribution
		{
			get { return _cognateSoundCorrespondenceProbabilityDistribution; }
			set
			{
				Set(nameof(CognateSoundCorrespondenceProbabilityDistribution), ref _cognateSoundCorrespondenceProbabilityDistribution,
					value);
			}
		}

		public double DefaultSoundCorrespondenceProbability
		{
			get { return _defaultSoundCorrespondenceProb; }
			set { Set(nameof(DefaultSoundCorrespondenceProbability), ref _defaultSoundCorrespondenceProb, value); }
		}

		public int CognateCount
		{
			get { return _cognateCount; }
			set { Set(nameof(CognateCount), ref _cognateCount, value); }
		}
	}
}
