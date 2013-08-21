using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Applications.Services;
using SIL.Cog.Domain;
using SIL.Collections;
using SIL.Machine;

namespace SIL.Cog.Applications.ViewModels
{
	public class WordViewModel : ViewModelBase, IDataErrorInfo
	{
		public delegate WordViewModel Factory(Word word);

		private readonly IAnalysisService _analysisService; 
		private readonly Word _word;
		private BindableList<WordSegmentViewModel> _segments;
		private readonly SenseViewModel _sense;
		private bool _isValid;
		private readonly SimpleMonitor _monitor;
		private readonly ICommand _showInWordListsCommand;
		private readonly IBusyService _busyService;

		public WordViewModel(IBusyService busyService, IAnalysisService analysisService, Word word)
		{
			_busyService = busyService;
			_analysisService = analysisService;
			_sense = new SenseViewModel(word.Sense);
			_word = word;
			LoadSegments();
			_monitor = new SimpleMonitor();
			_showInWordListsCommand = new RelayCommand(ShowInWordLists);
			_word.PropertyChanged += WordPropertyChanged;
		}

		private void ShowInWordLists()
		{
			Messenger.Default.Send(new SwitchViewMessage(typeof(WordListsViewModel), _word.Variety, _sense.DomainSense));
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
					segments.AddRange(GetSegments(prefixAnn));
				segments.Add(new WordSegmentViewModel("|"));
				segments.AddRange(GetSegments(_word.Stem));
				segments.Add(new WordSegmentViewModel("|"));
				Annotation<ShapeNode> suffixAnn = _word.Suffix;
				if (suffixAnn != null)
					segments.AddRange(GetSegments(suffixAnn));
			}
			segments.CollectionChanged += SegmentsChanged;
			Set("Segments", ref _segments, segments);
			IsValid = _word.Shape != null && _word.Shape.Count > 0;
		}

		private IEnumerable<WordSegmentViewModel> GetSegments(Annotation<ShapeNode> ann)
		{
			foreach (Annotation<ShapeNode> child in ann.Children)
			{
				foreach (ShapeNode node in _word.Shape.GetNodes(child.Span))
					yield return new WordSegmentViewModel(node);
				if (child.Type() == CogFeatureSystem.SyllableType && child.Span.End != ann.Span.End
					&& !child.Span.End.Next.Type().IsOneOf(CogFeatureSystem.BoundaryType, CogFeatureSystem.ToneLetterType))
				{
					yield return new WordSegmentViewModel(".");
				}
			}
		}

		private void SegmentsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action != NotifyCollectionChangedAction.Add && e.Action != NotifyCollectionChangedAction.Move)
				return;

			_busyService.ShowBusyIndicatorUntilUpdated();
			int i = 0;
			int index = 0;
			while (!_segments[i].IsBoundary)
			{
				if (!_segments[i].IsNotInOriginal)
					index += _segments[i].DomainNode.OriginalStrRep().Length;
				i++;
			}
			_word.StemIndex = index;
			i++;
			while (!_segments[i].IsBoundary)
			{
				if (!_segments[i].IsNotInOriginal)
					index += _segments[i].DomainNode.OriginalStrRep().Length;
				i++;
			}
			_word.StemLength = index - _word.StemIndex;

			_analysisService.Segment(_word.Variety);
			Messenger.Default.Send(new DomainModelChangedMessage());
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
							return "The word contains invalid segments.";
						break;
				}

				return null;
			}
		}

		string IDataErrorInfo.Error
		{
			get { return null; }
		}

		internal Word DomainWord
		{
			get { return _word; }
		}
	}
}
