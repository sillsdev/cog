using System.Collections.ObjectModel;
using SIL.Collections;

namespace SIL.Cog
{
	public class WordPair : NotifyPropertyChangedBase
	{
		private readonly VarietyPair _varietyPair;
		private readonly Word _word1;
		private readonly Word _word2;
		private readonly ObservableCollection<string> _alignmentNotes;
		private bool _areCognatesActual;
		private bool _areCognatesPredicted;
		private double _phoneticSimilarityScore;

		public WordPair(VarietyPair varietyPair, Word word1, Word word2)
		{
			_varietyPair = varietyPair;
			_word1 = word1;
			_word2 = word2;
			_alignmentNotes = new ObservableCollection<string>();
		}

		public VarietyPair VarietyPair
		{
			get { return _varietyPair; }
		}

		public Word Word1
		{
			get { return _word1; }
		}

		public Word Word2
		{
			get { return _word2; }
		}

		public ObservableCollection<string> AlignmentNotes
		{
			get { return _alignmentNotes; }
		}

		public bool AreCognatesActual
		{
			get { return _areCognatesActual; }
			set
			{
				_areCognatesActual = value;
				OnPropertyChanged("AreCognatesActual");
			}
		}

		public bool AreCognatesPredicted
		{
			get { return _areCognatesPredicted; }
			set
			{
				_areCognatesPredicted = value;
				OnPropertyChanged("AreCognatesPredicted");
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
