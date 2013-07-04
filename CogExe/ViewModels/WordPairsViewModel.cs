using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Data;
using GalaSoft.MvvmLight;
using SIL.Collections;

namespace SIL.Cog.ViewModels
{
	public class WordPairsViewModel : ViewModelBase
	{
		private readonly BindableList<WordPairViewModel> _wordPairs;
		private ListCollectionView _wordPairsView;
		private readonly BindableList<WordPairViewModel> _selectedWordPairs;
		private readonly BindableList<WordPairViewModel> _selectedChangeWordPairs;

		public WordPairsViewModel(CogProject project, IEnumerable<WordPair> wordPairs, bool areVarietiesInOrder)
			: this(wordPairs.Select(pair => new WordPairViewModel(project, pair, areVarietiesInOrder)))
		{
		}

		public WordPairsViewModel()
			: this(Enumerable.Empty<WordPairViewModel>())
		{
		}

		private WordPairsViewModel(IEnumerable<WordPairViewModel> wordPairs)
		{
			_wordPairs = new BindableList<WordPairViewModel>(wordPairs);
			_wordPairs.CollectionChanged += _wordPairs_CollectionChanged;
			_selectedWordPairs = new BindableList<WordPairViewModel>();
			_selectedChangeWordPairs = new BindableList<WordPairViewModel>();
		}

		private void _wordPairs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			_selectedWordPairs.Clear();
			_selectedChangeWordPairs.Clear();
		}

		public ObservableList<WordPairViewModel> WordPairs
		{
			get { return _wordPairs; }
		}

		public ICollectionView WordPairsView
		{
			get
			{
				if (_wordPairsView == null)
				{
					_wordPairsView = new ListCollectionView(_wordPairs);
					_wordPairsView.SortDescriptions.Add(new SortDescription("PhoneticSimilarityScore", ListSortDirection.Descending));
				}
				return _wordPairsView;
			}
		}

		public ObservableList<WordPairViewModel> SelectedWordPairs
		{
			get { return _selectedWordPairs; }
		}

		public ObservableList<WordPairViewModel> SelectedChangeWordPairs
		{
			get { return _selectedChangeWordPairs; }
		}

		public string SelectedWordPairsText
		{
			get
			{
				bool first = true;
				var sb = new StringBuilder();
				foreach (WordPairViewModel pair in _selectedWordPairs.OrderByDescending(wp => wp.PhoneticSimilarityScore))
				{
					if (!first)
						sb.AppendLine();

					sb.Append(pair.Sense.Gloss);
					if (!string.IsNullOrEmpty(pair.Sense.Category))
						sb.AppendFormat(" ({0})", pair.Sense.Category);
					sb.AppendLine();

					string prefix1 = pair.PrefixNode.StrRep1;
					string prefix2 = pair.PrefixNode.StrRep2;
					string suffix1 = pair.SuffixNode.StrRep1;
					string suffix2 = pair.SuffixNode.StrRep2;

					if (prefix1.Length > 0 || prefix2.Length > 0)
					{
						sb.Append(PadString(prefix1, prefix2, ""));
						sb.Append(" ");
					}
					sb.Append("|");
					bool firstAlignedNode = true;
					foreach (AlignedNodeViewModel an in pair.AlignedNodes)
					{
						if (!firstAlignedNode)
							sb.Append(" ");
						sb.Append(PadString(an.StrRep1, an.StrRep2, an.Note));
						firstAlignedNode = false;
					}
					sb.Append("|");
					if (suffix1.Length > 0 || suffix2.Length > 0)
					{
						sb.Append(" ");
						sb.Append(PadString(suffix1, suffix2, ""));
					}
					sb.AppendLine();

					if (prefix1.Length > 0 || prefix2.Length > 0)
					{
						sb.Append(PadString(prefix2, prefix1, ""));
						sb.Append(" ");
					}
					sb.Append("|");
					firstAlignedNode = true;
					foreach (AlignedNodeViewModel an in pair.AlignedNodes)
					{
						if (!firstAlignedNode)
							sb.Append(" ");
						sb.Append(PadString(an.StrRep2, an.StrRep1, an.Note));
						firstAlignedNode = false;
					}
					sb.Append("|");
					if (suffix1.Length > 0 || suffix2.Length > 0)
					{
						sb.Append(" ");
						sb.Append(PadString(suffix2, suffix1, ""));
					}
					sb.AppendLine();

					if (prefix1.Length > 0 || prefix2.Length > 0)
					{
						sb.Append(PadString("", prefix1, prefix2));
						sb.Append(" ");
					}
					sb.Append(" ");
					firstAlignedNode = true;
					foreach (AlignedNodeViewModel an in pair.AlignedNodes)
					{
						if (!firstAlignedNode)
							sb.Append(" ");
						sb.Append(PadString(an.Note, an.StrRep1, an.StrRep2));
						firstAlignedNode = false;
					}
					sb.Append(" ");
					if (suffix1.Length > 0 || suffix2.Length > 0)
					{
						sb.Append(" ");
						sb.Append(PadString("", suffix1, suffix2));
					}
					sb.AppendLine();

					sb.AppendFormat("Similarity: {0:p}", pair.PhoneticSimilarityScore);
					sb.AppendLine();
					first = false;
				}
				return sb.ToString();
			}
		}

		private static string PadString(string str, params string[] strs)
		{
			int len = str.DisplayLength();
			int maxLen = strs.Select(s => s.DisplayLength()).Concat(len).Max();
			var sb = new StringBuilder();
			sb.Append(str);
			for (int i = 0; i < maxLen - len; i++)
				sb.Append(" ");

			return sb.ToString();
		}
	}
}
