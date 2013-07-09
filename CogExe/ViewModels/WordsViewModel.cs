using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Data;
using SIL.Cog.Services;
using SIL.Collections;

namespace SIL.Cog.ViewModels
{
	public class WordsViewModel : CogViewModelBase
	{
		private readonly ReadOnlyMirroredCollection<Word, WordViewModel> _words;
		private ListCollectionView _wordsView;
		private readonly BindableList<WordViewModel> _selectedWords;
		private readonly BindableList<WordViewModel> _selectedSegmentWords;

		public WordsViewModel(IBusyService busyService, CogProject project, Variety variety)
		{
			_words = new ReadOnlyMirroredCollection<Word, WordViewModel>(variety.Words, word =>
				{
					var vm = new WordViewModel(busyService, project, word);
					vm.PropertyChanged += ChildPropertyChanged;
					return vm;
				}, vm => vm.ModelWord);
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

		public override void AcceptChanges()
		{
			base.AcceptChanges();
			ChildrenAcceptChanges(_words);
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

					sb.Append(word.ModelWord);
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
			get
			{
				if (_wordsView == null)
				{
					_wordsView = new ListCollectionView(_words);
					_wordsView.SortDescriptions.Add(new SortDescription("Sense.Gloss", ListSortDirection.Ascending));
				}
				return _wordsView;
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
