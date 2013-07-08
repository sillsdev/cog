using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Components;
using SIL.Collections;
using SIL.Machine;

namespace SIL.Cog.ViewModels
{
	public class WordViewModel : CogViewModelBase, IDataErrorInfo
	{
		private readonly CogProject _project; 
		private readonly Word _word;
		private BindableList<WordSegmentViewModel> _segments;
		private readonly SenseViewModel _sense;
		private bool _isValid;
		private readonly SimpleMonitor _monitor;
		private readonly ICommand _showInWordListsCommand;

		public WordViewModel(CogProject project, Word word)
		{
			_project = project;
			_sense = new SenseViewModel(word.Sense);
			_word = word;
			LoadSegments();
			_monitor = new SimpleMonitor();
			_showInWordListsCommand = new RelayCommand(ShowInWordLists);
			_word.PropertyChanged += WordPropertyChanged;
		}

		private void ShowInWordLists()
		{
			Messenger.Default.Send(new Message(MessageType.SwitchView, new SwitchViewData(typeof(WordListsViewModel), _word.Variety, _sense.ModelSense)));
		}

		private void WordPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "Shape":
					if (_monitor.Busy)
						return;
					LoadSegments();
					break;
			}
		}

		public ICommand ShowInWordListsCommand
		{
			get { return _showInWordListsCommand; }
		}

		private void LoadSegments()
		{
			var segments = new BindableList<WordSegmentViewModel>();
			if (_word.Shape != null && _word.Shape.Count > 0)
			{
				Annotation<ShapeNode> prefixAnn = _word.Prefix;
				if (prefixAnn != null)
					segments.AddRange(_word.Shape.GetNodes(prefixAnn.Span).Select(node => new WordSegmentViewModel(node)));
				segments.Add(new WordSegmentViewModel());
				segments.AddRange(_word.Shape.GetNodes(_word.Stem.Span).Select(node => new WordSegmentViewModel(node)));
				segments.Add(new WordSegmentViewModel());
				Annotation<ShapeNode> suffixAnn = _word.Suffix;
				if (suffixAnn != null)
					segments.AddRange(_word.Shape.GetNodes(suffixAnn.Span).Select(node => new WordSegmentViewModel(node)));
			}
			segments.CollectionChanged += SegmentsChanged;
			Set("Segments", ref _segments, segments);
			IsValid = _word.Shape != null && _word.Shape.Count > 0;
		}

		private void SegmentsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action != NotifyCollectionChangedAction.Add && e.Action != NotifyCollectionChangedAction.Move)
				return;

			int i = 0;
			int index = 0;
			while (!_segments[i].IsBoundary)
			{
				index += _segments[i].OriginalStrRep.Length;
				i++;
			}
			_word.StemIndex = index;
			i++;
			while (!_segments[i].IsBoundary)
			{
				index += _segments[i].OriginalStrRep.Length;
				i++;
			}
			_word.StemLength = index - _word.StemIndex;

			var pipeline = new Pipeline<Variety>(_project.GetVarietyInitProcessors());
			pipeline.Process(_word.Variety.ToEnumerable());

			IsChanged = true;
		}

		public SenseViewModel Sense
		{
			get { return _sense; }
		}

		public string StrRep
		{
			get { return _word.StrRep; }
		}

		public bool IsValid
		{
			get { return _isValid; }
			set { Set(() => IsValid, ref _isValid, value); }
		}

		public ObservableList<WordSegmentViewModel> Segments
		{
			get { return _segments; }
		}

		string IDataErrorInfo.this[string columnName]
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

		string IDataErrorInfo.Error
		{
			get { return null; }
		}

		internal Word ModelWord
		{
			get { return _word; }
		}
	}
}
