using System.Collections.ObjectModel;
using SIL.Collections;

namespace SIL.Cog
{
	public class WordPair : NotifyPropertyChangedBase
	{
		private readonly Word _word1;
		private readonly Word _word2;
		private readonly ObservableCollection<string> _alignmentNotes;
		private bool _areCognateActual;
		private bool _areCognatePredicted;
		private double _phoneticSimilarityScore;

		public WordPair(Word word1, Word word2)
		{
			_word1 = word1;
			_word2 = word2;
			_alignmentNotes = new ObservableCollection<string>();
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

		public Sense Sense
		{
			get { return _word1.Sense; }
		}

		public ObservableCollection<string> AlignmentNotes
		{
			get { return _alignmentNotes; }
		}

		public bool AreCognateActual
		{
			get { return _areCognateActual; }
			set
			{
				_areCognateActual = value;
				OnPropertyChanged("AreCognateActual");
			}
		}

		public bool AreCognatePredicted
		{
			get { return _areCognatePredicted; }
			set
			{
				_areCognatePredicted = value;
				OnPropertyChanged("AreCognatePredicted");
			}
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
	}
}
