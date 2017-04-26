using SIL.Collections;

namespace SIL.Cog.Domain
{
	public class WordPair : ObservableObject
	{
		private bool _cognacy;
		private bool? _actualCognacy;
		private bool _predictedCognacy;
		private double _phoneticSimilarityScore;
		private double _predictedCognacyScore;

		public WordPair(Word word1, Word word2)
		{
			Word1 = word1;
			Word2 = word2;
			AlignmentNotes = new ObservableList<string>();
		}

		public VarietyPair VarietyPair { get; internal set; }

		public Word Word1 { get; }

		public Word Word2 { get; }

		public Word GetWord(Variety v)
		{
			if (VarietyPair.Variety1 == v)
				return Word1;
			return Word2;
		}

		public Meaning Meaning => Word1.Meaning;

		public ObservableList<string> AlignmentNotes { get; }

		public bool Cognacy
		{
			get { return _cognacy; }
			private set { Set(() => Cognacy, ref _cognacy, value); }
		}

		public bool? ActualCognacy
		{
			get { return _actualCognacy; }
			set
			{
				if (Set(() => ActualCognacy, ref _actualCognacy, value))
					UpdateCognacy();
			}
		}

		public bool PredictedCognacy
		{
			get { return _predictedCognacy; }
			set
			{
				if (Set(() => PredictedCognacy, ref _predictedCognacy, value))
					UpdateCognacy();
			}
		}

		private void UpdateCognacy()
		{
			Cognacy = _actualCognacy ?? _predictedCognacy;
		}

		public double PhoneticSimilarityScore
		{
			get { return _phoneticSimilarityScore; }
			set { Set(() => PhoneticSimilarityScore, ref _phoneticSimilarityScore, value); }
		}

		public double PredictedCognacyScore
		{
			get { return _predictedCognacyScore; }
			set { Set(() => PredictedCognacyScore, ref _predictedCognacyScore, value); }
		}
	}
}
