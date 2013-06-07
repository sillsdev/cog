using SIL.Cog.Statistics;
using SIL.Collections;

namespace SIL.Cog
{
	public class VarietyPair : NotifyPropertyChangedBase
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

		public VarietyPair(Variety variety1, Variety variety2)
		{
			_variety1 = variety1;
			_variety2 = variety2;
			_wordPairs = new WordPairCollection(this);
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
			set
			{
				_phoneticSimilarityScore = value;
				OnPropertyChanged("PhoneticSimilarityScore");
			}
		}

		public double LexicalSimilarityScore
		{
			get { return _lexicalSimilarityScore; }
			set
			{
				_lexicalSimilarityScore = value;
				OnPropertyChanged("LexicalSimilarityScore");
			}
		}

		public double Significance
		{
			get { return _significance; }
			set
			{
				_significance = value;
				OnPropertyChanged("Significance");
			}
		}

		public double Precision
		{
			get { return _precision; }
			set
			{
				_precision = value;
				OnPropertyChanged("Precision");
			}
		}

		public double Recall
		{
			get { return _recall; }
			set
			{
				_recall = value;
				OnPropertyChanged("Recall");
			}
		}

		public IConditionalProbabilityDistribution<SoundContext, Ngram> SoundChangeProbabilityDistribution
		{
			get { return _soundChangeProbabilityDistribution; }
			set
			{
				_soundChangeProbabilityDistribution = value;
				OnPropertyChanged("SoundChangeProbabilityDistribution");
			}
		}

		public ConditionalFrequencyDistribution<SoundContext, Ngram> SoundChangeFrequencyDistribution
		{
			get { return _soundFreqDist; }
			set
			{
				_soundFreqDist = value;
				OnPropertyChanged("SoundChangeFrequencyDistribution");
			}
		}

		public double DefaultCorrespondenceProbability
		{
			get { return _defaultCorrProb; }
			set
			{
				_defaultCorrProb = value;
				OnPropertyChanged("DefaultCorrespondenceProbability");
			}
		}
	}
}
