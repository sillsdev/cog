using SIL.Collections;

namespace SIL.Cog.Domain
{
	public class WordPair : ObservableObject
	{
		private readonly Word _word1;
		private readonly Word _word2;
		private readonly ObservableList<string> _alignmentNotes;
		private bool _areCognateActual;
		private bool _areCognatePredicted;
		private double _phoneticSimilarityScore;
		private double _cognicityScore;

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

		public bool AreCognateActual
		{
			get { return _areCognateActual; }
			set { Set(() => AreCognateActual, ref _areCognateActual, value); }
		}

		public bool AreCognatePredicted
		{
			get { return _areCognatePredicted; }
			set { Set(() => AreCognatePredicted, ref _areCognatePredicted, value); }
		}

		public double PhoneticSimilarityScore
		{
			get { return _phoneticSimilarityScore; }
			set { Set(() => PhoneticSimilarityScore, ref _phoneticSimilarityScore, value); }
		}

		public double CognicityScore
		{
			get { return _cognicityScore; }
			set { Set(() => CognicityScore, ref _cognicityScore, value); }
		}
	}
}
