using System.Collections.Generic;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;

namespace SIL.Cog.ViewModels
{
	public class VarietySenseViewModel : ViewModelBase
	{
		private readonly Sense _modelSense;
		private readonly ObservableCollection<Word> _modelWords;
		private readonly ViewModelCollection<WordViewModel, Word> _words;

		public VarietySenseViewModel(Sense sense, IEnumerable<Word> words)
		{
			_modelSense = sense;
			_modelWords = new ObservableCollection<Word>(words);
			_words = new ViewModelCollection<WordViewModel, Word>(_modelWords, word => new WordViewModel(word));
		}

		public string Gloss
		{
			get { return _modelSense.Gloss; }
		}

		public string Category
		{
			get { return _modelSense.Category; }
		}

		public ObservableCollection<WordViewModel> Words
		{
			get { return _words; }
		}

		public Sense ModelSense
		{
			get { return _modelSense; }
		}

		public ObservableCollection<Word> ModelWords
		{
			get { return _modelWords; }
		}
	}
}
