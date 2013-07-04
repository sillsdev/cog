using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using SIL.Cog.Services;
using SIL.Collections;

namespace SIL.Cog.ViewModels
{
	public class VarietiesVarietyViewModel : VarietyViewModel
	{
		private readonly CogProject _project;
		private readonly IDialogService _dialogService;
		private readonly BindableList<VarietySegmentViewModel> _segments;
		private readonly ReadOnlyObservableList<VarietySegmentViewModel> _readOnlySegments;
		private double _maxSegProb;
		private ListCollectionView _segmentsView;
		private readonly ReadOnlyMirroredList<Sense, SenseViewModel> _senses;
		private readonly ReadOnlyMirroredCollection<Word, WordViewModel> _words;
		private ListCollectionView _wordsView;
		private readonly ReadOnlyMirroredList<Affix, AffixViewModel> _affixes;
		private VarietySegmentViewModel _currentSegment;
		private AffixViewModel _currentAffix;
		private readonly BindableList<WordViewModel> _selectedWords;
		private readonly ICommand _newAffixCommand;
		private readonly ICommand _editAffixCommand;
		private readonly ICommand _removeAffixCommand;
 
		public VarietiesVarietyViewModel(IDialogService dialogService, CogProject project, Variety variety)
			: base(variety)
		{
			_project = project;
			_dialogService = dialogService;
			_segments = new BindableList<VarietySegmentViewModel>(variety.SegmentFrequencyDistribution == null ? Enumerable.Empty<VarietySegmentViewModel>()
				: variety.SegmentFrequencyDistribution.ObservedSamples.Select(seg => new VarietySegmentViewModel(variety, seg)));
			_maxSegProb = _segments.Select(seg => seg.Probability).Concat(0).Max();
			_readOnlySegments = new ReadOnlyObservableList<VarietySegmentViewModel>(_segments);
			variety.PropertyChanged += VarietyPropertyChanged;
			_senses = new ReadOnlyMirroredList<Sense, SenseViewModel>(_project.Senses, sense => new SenseViewModel(sense), vm => vm.ModelSense);
			_words = new ReadOnlyMirroredCollection<Word, WordViewModel>(variety.Words, word =>
				{
					var vm = new WordViewModel(project, _senses[word.Sense], word);
					vm.PropertyChanged += ChildPropertyChanged;
					return vm;
				}, vm => vm.ModelWord);
			variety.Words.CollectionChanged += WordsChanged;

			_selectedWords = new BindableList<WordViewModel>();
			_affixes = new ReadOnlyMirroredList<Affix, AffixViewModel>(ModelVariety.Affixes, affix => new AffixViewModel(affix), vm => vm.ModelAffix);
			_newAffixCommand = new RelayCommand(NewAffix);
			_editAffixCommand = new RelayCommand(EditAffix, CanEditAffix);
			_removeAffixCommand = new RelayCommand(RemoveAffix, CanRemoveAffix);
		}

		private void VarietyPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "SegmentProbabilityDistribution":
					var variety = (Variety) sender;
					Segment curSeg = null;
					if (CurrentSegment != null)
						curSeg = CurrentSegment.ModelSegment;
					using (_segments.BulkUpdate())
					{
						_segments.Clear();
						if (variety.SegmentFrequencyDistribution != null)
							_segments.AddRange(variety.SegmentFrequencyDistribution.ObservedSamples.Select(seg => new VarietySegmentViewModel(variety, seg)));
					}
					MaxSegmentProbability = _segments.Select(seg => seg.Probability).Concat(0).Max();
					if (curSeg != null)
						CurrentSegment = _segments.FirstOrDefault(vm => vm.ModelSegment.Equals(curSeg));
					break;
			}
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

		private void NewAffix()
		{
			var vm = new EditAffixViewModel(_project);
			if (_dialogService.ShowDialog(this, vm) == true)
			{
				var affix = new Affix(vm.StrRep, vm.Type == AffixViewModelType.Prefix ? AffixType.Prefix : AffixType.Suffix, vm.Category);
				ModelVariety.Affixes.Add(affix);
				_project.Segmenter.Segment(affix);
				CurrentAffix = _affixes.Single(a => a.ModelAffix == affix);
				IsChanged = true;
			}
		}

		private bool CanEditAffix()
		{
			return CurrentAffix != null;
		}

		private void EditAffix()
		{
			var vm = new EditAffixViewModel(_project, _currentAffix.ModelAffix);
			if (_dialogService.ShowDialog(this, vm) == true)
			{
				var affix = new Affix(vm.StrRep, vm.Type == AffixViewModelType.Prefix ? AffixType.Prefix : AffixType.Suffix, vm.Category);
				int index = ModelVariety.Affixes.IndexOf(_currentAffix.ModelAffix);
				ModelVariety.Affixes[index] = affix;
				_project.Segmenter.Segment(affix);
				CurrentAffix = _affixes.Single(a => a.ModelAffix == affix);
				IsChanged = true;
			}
		}

		private void RemoveAffix()
		{
			ModelVariety.Affixes.Remove(CurrentAffix.ModelAffix);
			IsChanged = true;
		}

		private bool CanRemoveAffix()
		{
			return CurrentAffix != null;
		}

		public override void AcceptChanges()
		{
			base.AcceptChanges();
			ChildrenAcceptChanges(_words);
		}

		public double MaxSegmentProbability
		{
			get { return _maxSegProb; }
			private set { Set(() => MaxSegmentProbability, ref _maxSegProb, value); }
		}

		public ReadOnlyObservableList<VarietySegmentViewModel> Segments
		{
			get { return _readOnlySegments; }
		}

		public ICollectionView SegmentsView
		{
			get
			{
				if (_segmentsView == null)
					_segmentsView = new ListCollectionView(_segments);
				return _segmentsView;
			}
		}

		public ReadOnlyObservableList<SenseViewModel> Senses
		{
			get { return _senses; }
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
					Debug.Assert(_wordsView.GroupDescriptions != null);
					_wordsView.GroupDescriptions.Add(new PropertyGroupDescription("Sense"));
					_wordsView.SortDescriptions.Add(new SortDescription("Sense.Gloss", ListSortDirection.Ascending));
				}
				return _wordsView;
			}
		}

		public ReadOnlyObservableList<AffixViewModel> Affixes
		{
			get { return _affixes; }
		}

		public ObservableList<WordViewModel> SelectedWords
		{
			get { return _selectedWords; }
		}

		public AffixViewModel CurrentAffix
		{
			get { return _currentAffix; }
			set { Set(() => CurrentAffix, ref _currentAffix, value); }
		}

		public VarietySegmentViewModel CurrentSegment
		{
			get { return _currentSegment; }
			set
			{
				if (Set(() => CurrentSegment, ref _currentSegment, value))
				{
					_selectedWords.Clear();
					foreach (WordViewModel word in _words)
					{
						bool selected = false;
						foreach (WordSegmentViewModel segment in word.Segments)
						{
							segment.IsSelected = _currentSegment != null && segment.StrRep == _currentSegment.StrRep;
							if (segment.IsSelected)
								selected = true;
						}

						if (selected)
							_selectedWords.Add(word);
					}
				}
			}
		}

		public ICommand NewAffixCommand
		{
			get { return _newAffixCommand; }
		}

		public ICommand EditAffixCommand
		{
			get { return _editAffixCommand; }
		}

		public ICommand RemoveAffixCommand
		{
			get { return _removeAffixCommand; }
		}
	}
}
