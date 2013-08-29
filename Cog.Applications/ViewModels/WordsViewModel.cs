using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using SIL.Cog.Applications.Services;
using SIL.Collections;

namespace SIL.Cog.Applications.ViewModels
{
	public class WordsViewModel : ViewModelBase
	{
		public delegate WordsViewModel Factory(IObservableList<WordViewModel> words);

		private readonly IBusyService _busyService;
		private readonly ReadOnlyObservableList<WordViewModel> _words; 
		private ICollectionView _wordsView;
		private readonly BindableList<WordViewModel> _selectedWords;
		private readonly BindableList<WordViewModel> _selectedSegmentWords;
		private SortDescription? _deferredSortDesc;
		private WordViewModel _startWord;
		private readonly SimpleMonitor _selectedWordsMonitor;

		public WordsViewModel(IBusyService busyService, IObservableList<WordViewModel> words)
		{
			_busyService = busyService;
			var readonlyWords = words as ReadOnlyObservableList<WordViewModel>;
			_words = readonlyWords ?? new ReadOnlyObservableList<WordViewModel>(words);
			words.CollectionChanged += WordsChanged;
			_selectedWords = new BindableList<WordViewModel>();
			_selectedWords.CollectionChanged += _selectedWords_CollectionChanged;
			_selectedSegmentWords = new BindableList<WordViewModel>();
			_selectedWordsMonitor = new SimpleMonitor();
		}

		private void WordsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					AddWords(e.NewItems.Cast<WordViewModel>());
					break;
				case NotifyCollectionChangedAction.Remove:
					RemoveWords(e.OldItems.Cast<WordViewModel>());
					break;
				case NotifyCollectionChangedAction.Replace:
					RemoveWords(e.OldItems.Cast<WordViewModel>());
					AddWords(e.NewItems.Cast<WordViewModel>());
					break;
				case NotifyCollectionChangedAction.Reset:
					_selectedWords.Clear();
					using (_selectedSegmentWords.BulkUpdate())
					{
						_selectedSegmentWords.Clear();
						AddWords(_words);
					}
					break;
			}
			ResetSearch();
		}

		private void AddWords(IEnumerable<WordViewModel> words)
		{
			foreach (WordViewModel word in words)
			{
				if (word.Segments.Any(s => s.IsSelected))
					_selectedSegmentWords.Add(word);
			}
		}

		private void RemoveWords(IEnumerable<WordViewModel> words)
		{
			foreach (WordViewModel word in words)
			{
				_selectedSegmentWords.Remove(word);
				_selectedWords.Remove(word);
			}
		}

		internal void UpdateSort(string propertyName, ListSortDirection sortDirection)
		{
			var sortDesc = new SortDescription(propertyName, sortDirection);
			if (_wordsView == null)
				_deferredSortDesc = sortDesc;
			else
				UpdateSort(sortDesc);
		}

		private void UpdateSort(SortDescription sortDesc)
		{
			_busyService.ShowBusyIndicatorUntilUpdated();
			if (_wordsView.SortDescriptions.Count == 0)
				_wordsView.SortDescriptions.Add(sortDesc);
			else
				_wordsView.SortDescriptions[0] = sortDesc;
		}

		internal bool FindNext(FindField field, string str)
		{
			if (_words.Count == 0)
			{
				ResetSearch();
				return false;
			}
			if (_selectedWords.Count == 0)
			{
				_startWord = _wordsView.Cast<WordViewModel>().Last();
			}
			else if (_startWord == null)
			{
				_startWord = _selectedWords[0];
			}
			else if (_selectedWords.Contains(_startWord))
			{
				ResetSearch();
				return false;
			}

			List<WordViewModel> words = _wordsView.Cast<WordViewModel>().ToList();
			WordViewModel curWord = _selectedWords.Count == 0 ? _startWord : _selectedWords[0];
			int wordIndex = words.IndexOf(curWord);
			do
			{
				wordIndex = (wordIndex + 1) % words.Count;
				curWord = words[wordIndex];
				bool match = false;
				switch (field)
				{
					case FindField.Form:
						match = curWord.StrRep.Contains(str);
						break;

					case FindField.Sense:
						match = curWord.Sense.Gloss.Contains(str);
						break;
				}
				if (match)
				{
					using (_selectedWordsMonitor.Enter())
					{
						_selectedWords.Clear();
						_selectedWords.Add(curWord);
					}
					return true;
				}
			}
			while (_startWord != curWord);
			ResetSearch();
			return false;
		}

		internal void ResetSearch()
		{
			_startWord = null;
		}

		private void _selectedWords_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (!_selectedWordsMonitor.Busy)
				ResetSearch();
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
			set
			{
				if (Set(() => WordsView, ref _wordsView, value) && _deferredSortDesc != null)
				{
					UpdateSort(_deferredSortDesc.Value);
					_deferredSortDesc = null;
				}
			}
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
