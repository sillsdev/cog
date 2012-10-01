using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using GalaSoft.MvvmLight;
using SIL.Machine;
using SIL.Collections;

namespace SIL.Cog.ViewModels
{
	public class WordViewModel : ViewModelBase, IDataErrorInfo
	{
		private readonly Word _word;
		private readonly ObservableCollection<WordSegmentViewModel> _segments; 

		public WordViewModel(Word word)
		{
			_word = word;
			_segments = new ObservableCollection<WordSegmentViewModel>();
			Annotation<ShapeNode> prefixAnn = _word.Annotations.SingleOrDefault(ann => ann.Type() == CogFeatureSystem.PrefixType);
			if (prefixAnn != null)
				_segments.AddRange(_word.Shape.GetNodes(prefixAnn.Span).Select(node => new WordSegmentViewModel(node)));
			_segments.Add(new WordSegmentViewModel("|"));
			Annotation<ShapeNode> stemAnn = _word.Annotations.SingleOrDefault(ann => ann.Type() == CogFeatureSystem.StemType);
			_segments.AddRange(stemAnn != null ? _word.Shape.GetNodes(stemAnn.Span).Select(node => new WordSegmentViewModel(node)) : _word.Shape.Select(node => new WordSegmentViewModel(node)));
			_segments.Add(new WordSegmentViewModel("|"));
			Annotation<ShapeNode> suffixAnn = _word.Annotations.SingleOrDefault(ann => ann.Type() == CogFeatureSystem.SuffixType);
			if (suffixAnn != null)
				_segments.AddRange(_word.Shape.GetNodes(suffixAnn.Span).Select(node => new WordSegmentViewModel(node)));
			_segments.CollectionChanged += SegmentsChanged;
		}

		private void SegmentsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{

		}

		public string StrRep
		{
			get { return _word.StrRep; }
		}

		public ObservableCollection<WordSegmentViewModel> Segments
		{
			get { return _segments; }
		}

		public string this[string columnName]
		{
			get
			{
				switch (columnName)
				{
					case "StrRep":
						if (_word.Shape.Count == 0)
							return "The word contains invalid segments";
						break;
				}

				return null;
			}
		}

		public string Error
		{
			get { return null; }
		}
	}
}
