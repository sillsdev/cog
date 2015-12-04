using SIL.Collections;

namespace SIL.Cog.Domain
{
	public class WordPair : ObservableObject
	{
		private readonly Word _word1;
		private readonly Word _word2;
		private readonly ObservableList<string> _alignmentNotes;
		private bool _cognacy;
		private bool? _actualCognacy;
		private bool _predictedCognacy;
		private double _phoneticSimilarityScore;
		private double _predictedCognacyScore;

		public WordPair(Word word1, Word word2)
		{
			_word1 = word1;
			_word2 = word2;
			_alignmentNotes = new ObservableList<string>();
		}

		public VarietyPair VarietyPair { get; internal set; }

		public Word Word1
		{
			get { return _word1; }
		}

		public Word Word2
		{
			get { return _word2; }
		}

		public Word GetWord(Variety v)
		{
			if (VarietyPair.Variety1 == v)
				return _word1;
			return _word2;
		}

		public Meaning Meaning
		{
			get { return _word1.Meaning; }
		}

		public ObservableList<string> AlignmentNotes
		{
			get { return _alignmentNotes; }
		}

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
