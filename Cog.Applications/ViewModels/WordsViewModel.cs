using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using SIL.Cog.Applications.Services;
using SIL.Cog.Domain;
using SIL.Collections;

namespace SIL.Cog.Applications.ViewModels
{
	public class WordsViewModel : ViewModelBase
	{
		public delegate WordsViewModel Factory(Variety variety);

		private readonly IBusyService _busyService;
		private readonly ReadOnlyMirroredCollection<Word, WordViewModel> _words;
		private ICollectionView _wordsView;
		private readonly BindableList<WordViewModel> _selectedWords;
		private readonly BindableList<WordViewModel> _selectedSegmentWords;
		private SortDescription? _deferredSortDesc;

		public WordsViewModel(IBusyService busyService, WordViewModel.Factory wordFactory, Variety variety)
		{
			_busyService = busyService;
			_words = new ReadOnlyMirroredCollection<Word, WordViewModel>(variety.Words, word => wordFactory(word), vm => vm.DomainWord);
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
