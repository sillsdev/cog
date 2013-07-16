using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Applications.Services;
using SIL.Cog.Domain;
using SIL.Collections;

namespace SIL.Cog.Applications.ViewModels
{
	public class VarietiesVarietyViewModel : VarietyViewModel
	{
		private readonly CogProject _project;
		private readonly IDialogService _dialogService;
		private readonly BindableList<VarietySegmentViewModel> _segments;
		private readonly ReadOnlyObservableList<VarietySegmentViewModel> _readOnlySegments;
		private double _maxSegProb;
		private ListCollectionView _segmentsView;
		private readonly ReadOnlyMirroredList<Affix, AffixViewModel> _affixes;
		private VarietySegmentViewModel _currentSegment;
		private AffixViewModel _currentAffix;
		private readonly WordsViewModel _words;
		private readonly ICommand _newAffixCommand;
		private readonly ICommand _editAffixCommand;
		private readonly ICommand _removeAffixCommand;
 
		public VarietiesVarietyViewModel(IDialogService dialogService, IBusyService busyService, CogProject project, Variety variety)
			: base(variety)
		{
			_project = project;
			_dialogService = dialogService;
			_segments = new BindableList<VarietySegmentViewModel>(variety.SegmentFrequencyDistribution == null ? Enumerable.Empty<VarietySegmentViewModel>()
				: variety.SegmentFrequencyDistribution.ObservedSamples.Select(seg => new VarietySegmentViewModel(variety, seg)));
			_maxSegProb = _segments.Select(seg => seg.Probability).Concat(0).Max();
			_readOnlySegments = new ReadOnlyObservableList<VarietySegmentViewModel>(_segments);
			variety.PropertyChanged += VarietyPropertyChanged;
			_affixes = new ReadOnlyMirroredList<Affix, AffixViewModel>(DomainVariety.Affixes, affix => new AffixViewModel(affix), vm => vm.DomainAffix);
			_words = new WordsViewModel(busyService, project, variety);
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
						curSeg = CurrentSegment.DomainSegment;
					using (_segments.BulkUpdate())
					{
						_segments.Clear();
						if (variety.SegmentFrequencyDistribution != null)
							_segments.AddRange(variety.SegmentFrequencyDistribution.ObservedSamples.Select(seg => new VarietySegmentViewModel(variety, seg)));
					}
					MaxSegmentProbability = _segments.Select(seg => seg.Probability).Concat(0).Max();
					if (curSeg != null)
						CurrentSegment = _segments.FirstOrDefault(vm => vm.DomainSegment.Equals(curSeg));
					break;
			}
		}

		private void NewAffix()
		{
			var vm = new EditAffixViewModel(_project);
			if (_dialogService.ShowModalDialog(this, vm) == true)
			{
				var affix = new Affix(vm.StrRep, vm.Type == AffixViewModelType.Prefix ? AffixType.Prefix : AffixType.Suffix, vm.Category);
				Messenger.Default.Send(new DomainModelChangingMessage());
				DomainVariety.Affixes.Add(affix);
				_project.Segmenter.Segment(affix);
				CurrentAffix = _affixes.Single(a => a.DomainAffix == affix);
			}
		}

		private bool CanEditAffix()
		{
			return CurrentAffix != null;
		}

		private void EditAffix()
		{
			var vm = new EditAffixViewModel(_project, _currentAffix.DomainAffix);
			if (_dialogService.ShowModalDialog(this, vm) == true)
			{
				var affix = new Affix(vm.StrRep, vm.Type == AffixViewModelType.Prefix ? AffixType.Prefix : AffixType.Suffix, vm.Category);
				int index = DomainVariety.Affixes.IndexOf(_currentAffix.DomainAffix);
				Messenger.Default.Send(new DomainModelChangingMessage());
				DomainVariety.Affixes[index] = affix;
				_project.Segmenter.Segment(affix);
				CurrentAffix = _affixes.Single(a => a.DomainAffix == affix);
			}
		}

		private void RemoveAffix()
		{
			Messenger.Default.Send(new DomainModelChangingMessage());
			DomainVariety.Affixes.Remove(CurrentAffix.DomainAffix);
		}

		private bool CanRemoveAffix()
		{
			return CurrentAffix != null;
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

		public WordsViewModel Words
		{
			get { return _words; }
		}

		public ReadOnlyObservableList<AffixViewModel> Affixes
		{
			get { return _affixes; }
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
					_words.SelectedSegmentWords.Clear();
					foreach (WordViewModel word in _words.Words)
					{
						bool selected = false;
						foreach (WordSegmentViewModel segment in word.Segments)
						{
							segment.IsSelected = _currentSegment != null && segment.StrRep == _currentSegment.StrRep;
							if (segment.IsSelected)
								selected = true;
						}

						if (selected)
							_words.SelectedSegmentWords.Add(word);
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
