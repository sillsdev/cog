using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;
using GalaSoft.MvvmLight;
using SIL.Cog.Applications.Services;
using SIL.Cog.Domain;
using SIL.Collections;

namespace SIL.Cog.Applications.ViewModels
{
	public class WordPairsViewModel : ViewModelBase
	{
		public delegate WordPairsViewModel Factory();

		private readonly IBusyService _busyService;
		private readonly BindableList<WordPairViewModel> _wordPairs;
		private ICollectionView _wordPairsView;
		private readonly BindableList<WordPairViewModel> _selectedWordPairs;
		private readonly BindableList<WordPairViewModel> _selectedCorrespondenceWordPairs;
		private SortDescription? _deferredSortDesc;

		public WordPairsViewModel(IBusyService busyService)
		{
			_busyService = busyService;
			_wordPairs = new BindableList<WordPairViewModel>();
			_wordPairs.CollectionChanged += _wordPairs_CollectionChanged;
			_selectedWordPairs = new BindableList<WordPairViewModel>();
			_selectedCorrespondenceWordPairs = new BindableList<WordPairViewModel>();
		}

		public bool IncludeVarietyNamesInSelectedText { get; set; }

		private void _wordPairs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			_selectedWordPairs.Clear();
			_selectedCorrespondenceWordPairs.Clear();
		}

		internal void UpdateSort(string propertyName, ListSortDirection sortDirection)
		{
			var sortDesc = new SortDescription(propertyName, sortDirection);
			if (_wordPairsView == null)
				_deferredSortDesc = sortDesc;
			else
				UpdateSort(sortDesc);
		}

		private void UpdateSort(SortDescription sortDesc)
		{
			_busyService.ShowBusyIndicatorUntilUpdated();
			if (_wordPairsView.SortDescriptions.Count == 0)
				_wordPairsView.SortDescriptions.Add(sortDesc);
			else
				_wordPairsView.SortDescriptions[0] = sortDesc;
		}

		public ObservableList<WordPairViewModel> WordPairs
		{
			get { return _wordPairs; }
		}

		public ICollectionView WordPairsView
		{
			get { return _wordPairsView; }
			set
			{
				if (Set(() => WordPairsView, ref _wordPairsView, value) && _deferredSortDesc != null)
				{
					UpdateSort(_deferredSortDesc.Value);
					_deferredSortDesc = null;
				}
			}
		}

		public ObservableList<WordPairViewModel> SelectedWordPairs
		{
			get { return _selectedWordPairs; }
		}

		public ObservableList<WordPairViewModel> SelectedCorrespondenceWordPairs
		{
			get { return _selectedCorrespondenceWordPairs; }
		}

		public string SelectedWordPairsText
		{
			get
			{
				int count = 0;
				var sb = new StringBuilder();
				foreach (WordPairViewModel pair in _wordPairsView)
				{
					if (!_selectedWordPairs.Contains(pair))
						continue;

					if (count > 0)
						sb.AppendLine();

					if (IncludeVarietyNamesInSelectedText)
					{
						sb.AppendFormat("{0} \u2192 {1}", pair.Variety1.Name, pair.Variety2.Name);
						sb.AppendLine();
					}
					sb.Append(pair.Sense.Gloss);
					if (!string.IsNullOrEmpty(pair.Sense.Category))
						sb.AppendFormat(" ({0})", pair.Sense.Category);
					sb.AppendLine();

					sb.Append(pair.DomainAlignment.ToString(pair.DomainWordPair.AlignmentNotes));

					sb.AppendFormat("Similarity: {0:p}", pair.PhoneticSimilarityScore);
					sb.AppendLine();
					count++;
					if (count == _selectedWordPairs.Count)
						break;
				}
				return sb.ToString();
			}
		}
	}
}
