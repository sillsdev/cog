using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Data;
using GalaSoft.MvvmLight;
using SIL.Cog.Applications.Services;
using SIL.Cog.Domain;
using SIL.Collections;

namespace SIL.Cog.Applications.ViewModels
{
	public class WordsViewModel : ViewModelBase
	{
		private readonly ReadOnlyMirroredCollection<Word, WordViewModel> _words;
		private readonly ListCollectionView _wordsView;
		private readonly BindableList<WordViewModel> _selectedWords;
		private readonly BindableList<WordViewModel> _selectedSegmentWords;

		public WordsViewModel(IBusyService busyService, IAnalysisService analysisService, Variety variety)
		{
			_words = new ReadOnlyMirroredCollection<Word, WordViewModel>(variety.Words, word => new WordViewModel(busyService, analysisService, word), vm => vm.DomainWord);
			_wordsView = new ListCollectionView(_words);
			variety.Words.CollectionChanged += WordsChanged;
			_selectedWords = new BindableList<WordViewModel>();
			_selectedSegmentWords = new BindableList<WordViewModel>();
		}

		private void WordsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			_selectedWords.Clear();
			foreach (WordViewModel word in _words)
			{
				if (word.Segments.Any(s => s.IsSelected))
					_selectedWords.Add(word);
			}
		}

		public string SelectedWordsText
		{
			get
			{
				var sb = new StringBuilder();
				int count = 0;
				foreach (WordViewModel word in _wordsView)
				{
					if (!_selectedWords.Contains(word))
						continue;

					if (count > 0)
						sb.AppendLine();

					sb.Append(word.Sense.Gloss);
					if (!string.IsNullOrEmpty(word.Sense.Category))
						sb.AppendFormat(" ({0})", word.Sense.Category);
					sb.AppendLine();

					sb.Append(word.DomainWord);
					sb.AppendLine();
					count++;
					if (count == _selectedWords.Count)
						break;
				}
				return sb.ToString();
			}
		}

		public ReadOnlyObservableList<WordViewModel> Words
		{
			get { return _words; }
		}

		public ICollectionView WordsView
		{
			get { return _wordsView; }
		}

		public ObservableList<WordViewModel> SelectedWords
		{
			get { return _selectedWords; }
		}

		public ObservableList<WordViewModel> SelectedSegmentWords
		{
			get { return _selectedSegmentWords; }
		}
	}
}
