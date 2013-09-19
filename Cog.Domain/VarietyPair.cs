using System.Collections.Generic;
using SIL.Cog.Domain.Statistics;
using SIL.Collections;

namespace SIL.Cog.Domain
{
	public class VarietyPair : ObservableObject
	{
		private readonly Variety _variety1;
		private readonly Variety _variety2;
		private readonly WordPairCollection _wordPairs; 
		private IConditionalProbabilityDistribution<SoundContext, Ngram> _soundChangeProbabilityDistribution;
		private ConditionalFrequencyDistribution<SoundContext, Ngram> _soundFreqDist;
		private double _defaultCorrProb;
		private double _phoneticSimilarityScore;
		private double _lexicalSimilarityScore;
		private double _significance;
		private double _precision;
		private double _recall;
		private readonly ReadOnlyDictionary<SyllablePosition, SoundCorrespondenceCollection> _soundCorrespondenceCollections;

		public VarietyPair(Variety variety1, Variety variety2)
		{
			_variety1 = variety1;
			_variety2 = variety2;
			_wordPairs = new WordPairCollection(this);

			_soundCorrespondenceCollections = new ReadOnlyDictionary<SyllablePosition, SoundCorrespondenceCollection>(new Dictionary<SyllablePosition, SoundCorrespondenceCollection>
				{
					{SyllablePosition.Onset, new SoundCorrespondenceCollection()},
					{SyllablePosition.Nucleus, new SoundCorrespondenceCollection()},
					{SyllablePosition.Coda, new SoundCorrespondenceCollection()}
				});
		}

		public Variety Variety1
		{
			get { return _variety1; }
		}

		public Variety Variety2
		{
			get { return _variety2; }
		}

		public WordPairCollection WordPairs
		{
			get { return _wordPairs; }
		}

		public Variety GetOtherVariety(Variety variety)
		{
			Variety otherVariety = null;
			if (_variety1 == variety)
				otherVariety = _variety2;
			else if (_variety2 == variety)
				otherVariety = _variety1;
			return otherVariety;
		}

		public double PhoneticSimilarityScore
		{
			get { return _phoneticSimilarityScore; }
			set { Set(() => PhoneticSimilarityScore, ref _phoneticSimilarityScore, value); }
		}

		public double LexicalSimilarityScore
		{
			get { return _lexicalSimilarityScore; }
			set { Set(() => LexicalSimilarityScore, ref _lexicalSimilarityScore, value); }
		}

		public double Significance
		{
			get { return _significance; }
			set { Set(() => Significance, ref _significance, value); }
		}

		public double Precision
		{
			get { return _precision; }
			set { Set(() => Precision, ref _precision, value); }
		}

		public double Recall
		{
			get { return _recall; }
			set { Set(() => Recall, ref _recall, value); }
		}

		public ReadOnlyDictionary<SyllablePosition, SoundCorrespondenceCollection> SoundCorrespondenceCollections
		{
			get { return _soundCorrespondenceCollections; }
		}

		public IConditionalProbabilityDistribution<SoundContext, Ngram> SoundChangeProbabilityDistribution
		{
			get { return _soundChangeProbabilityDistribution; }
			set { Set(() => SoundChangeProbabilityDistribution, ref _soundChangeProbabilityDistribution, value); }
		}

		public ConditionalFrequencyDistribution<SoundContext, Ngram> SoundChangeFrequencyDistribution
		{
			get { return _soundFreqDist; }
			set { Set(() => SoundChangeFrequencyDistribution, ref _soundFreqDist, value); }
		}

		public double DefaultCorrespondenceProbability
		{
			get { return _defaultCorrProb; }
			set { Set(() => DefaultCorrespondenceProbability, ref _defaultCorrProb, value); }
		}
	}
}
