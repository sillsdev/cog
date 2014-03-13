using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using SIL.Cog.Application.Collections;
using SIL.Cog.Application.Services;
using SIL.Cog.Domain;
using SIL.Collections;

namespace SIL.Cog.Application.ViewModels
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
		private WordPairViewModel _startWordPair;
		private readonly SimpleMonitor _selectedWordPairsMonitor;

		public WordPairsViewModel(IBusyService busyService)
		{
			_busyService = busyService;
			_wordPairs = new BindableList<WordPairViewModel>();
			_wordPairs.CollectionChanged += _wordPairs_CollectionChanged;
			_selectedWordPairs = new BindableList<WordPairViewModel>();
			_selectedWordPairs.CollectionChanged += _selectedWordPairs_CollectionChanged;
			_selectedCorrespondenceWordPairs = new BindableList<WordPairViewModel>();
			_selectedWordPairsMonitor = new SimpleMonitor();
		}

		public bool IncludeVarietyNamesInSelectedText { get; set; }

		private void _wordPairs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			_selectedWordPairs.Clear();
			_selectedCorrespondenceWordPairs.Clear();
			ResetSearch();
		}

		private void _selectedWordPairs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (!_selectedWordPairsMonitor.Busy)
				ResetSearch();
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
			_busyService.ShowBusyIndicatorUntilFinishDrawing();
			if (_wordPairsView.SortDescriptions.Count == 0)
				_wordPairsView.SortDescriptions.Add(sortDesc);
			else
				_wordPairsView.SortDescriptions[0] = sortDesc;
		}

		internal bool FindNext(FindField field, string str, bool wrap, bool startAtBeginning)
		{
			if (_wordPairs.Count == 0)
			{
				if (wrap)
					ResetSearch();
				return false;
			}
			if (!startAtBeginning && _selectedWordPairs.Count == 0)
			{
				_startWordPair = _wordPairsView.Cast<WordPairViewModel>().Last();
			}
			else if (_startWordPair == null)
			{
				_startWordPair = _selectedWordPairs[0];
			}
			else if (!startAtBeginning && _selectedWordPairs.Contains(_startWordPair))
			{
				if (wrap)
					ResetSearch();
				return false;
			}

			List<WordPairViewModel> wordPairs = _wordPairsView.Cast<WordPairViewModel>().ToList();
			WordPairViewModel curWordPair;
			if (startAtBeginning)
			{
				curWordPair = wordPairs[wordPairs.Count - 1];
				if (_startWordPair == curWordPair)
				{
					ResetSearch();
					return false;
				}
			}
			else
			{
				curWordPair = _selectedWordPairs.Count == 0 ? _startWordPair : _selectedWordPairs[0];
			}
			int wordPairIndex = wordPairs.IndexOf(curWordPair);
			do
			{
				wordPairIndex++;
				if (wordPairIndex == wordPairs.Count)
				{
					if (!wrap && !startAtBeginning && _startWordPair != curWordPair)
						return false;
					wordPairIndex = 0;
				}
				curWordPair = wordPairs[wordPairIndex];
				bool match = false;
				switch (field)
				{
					case FindField.Form:
						match = curWordPair.DomainWordPair.Word1.StrRep.Contains(str)
							|| curWordPair.DomainWordPair.Word2.StrRep.Contains(str);
						break;

					case FindField.Gloss:
						match = curWordPair.Meaning.Gloss.Contains(str);
						break;
				}
				if (match)
				{
					using (_selectedWordPairsMonitor.Enter())
					{
						_selectedWordPairs.Clear();
						_selectedWordPairs.Add(curWordPair);
					}
					return true;
				}
			}
			while (_startWordPair != curWordPair);
			if (wrap)
				ResetSearch();
			return false;
		}

		internal bool IsSearching
		{
			get { return _startWordPair != null; }
		}

		internal void ResetSearch()
		{
			_startWordPair = null;
		}

		internal void ClearPreviousSearchHit()
		{
			using (_selectedWordPairsMonitor.Enter())
				_selectedWordPairs.Clear();
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
					sb.Append(pair.Meaning.Gloss);
					if (!string.IsNullOrEmpty(pair.Meaning.Category))
						sb.AppendFormat(" ({0})", pair.Meaning.Category);
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
