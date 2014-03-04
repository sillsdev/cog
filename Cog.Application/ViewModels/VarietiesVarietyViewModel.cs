using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Application.Collections;
using SIL.Cog.Application.Services;
using SIL.Cog.Domain;
using SIL.Collections;
using SIL.Machine.Morphology;

namespace SIL.Cog.Application.ViewModels
{
	public class VarietiesVarietyViewModel : VarietyViewModel
	{
		public delegate VarietiesVarietyViewModel Factory(Variety variety);

		private readonly IProjectService _projectService;
		private readonly IDialogService _dialogService;
		private readonly BulkObservableList<VarietySegmentViewModel> _segments;
		private readonly ReadOnlyBindableList<VarietySegmentViewModel> _readOnlySegments;
		private double _maxSegProb;
		private readonly MirroredBindableList<Affix, AffixViewModel> _affixes;
		private VarietySegmentViewModel _selectedSegment;
		private AffixViewModel _selectedAffix;
		private readonly WordsViewModel _wordsViewModel;
		private readonly ICommand _newAffixCommand;
		private readonly ICommand _editAffixCommand;
		private readonly ICommand _removeAffixCommand;
		private readonly MirroredBindableCollection<Word, WordViewModel> _words; 
 
		public VarietiesVarietyViewModel(IProjectService projectService, IDialogService dialogService, WordsViewModel.Factory wordsFactory, WordViewModel.Factory wordFactory, Variety variety)
			: base(variety)
		{
			_projectService = projectService;
			_dialogService = dialogService;

			IEnumerable<Segment> segments = variety.SegmentFrequencyDistribution == null ? Enumerable.Empty<Segment>() : variety.SegmentFrequencyDistribution.ObservedSamples;

			_segments = new BulkObservableList<VarietySegmentViewModel>(segments.Select(seg => new VarietySegmentViewModel(this, seg)));
			_maxSegProb = _segments.Select(seg => seg.Probability).Concat(0).Max();
			_readOnlySegments = new ReadOnlyBindableList<VarietySegmentViewModel>(_segments);
			variety.PropertyChanged += variety_PropertyChanged;
			_affixes = new MirroredBindableList<Affix, AffixViewModel>(DomainVariety.Affixes, affix => new AffixViewModel(affix), vm => vm.DomainAffix);
			_words = new MirroredBindableCollection<Word, WordViewModel>(variety.Words, word =>
				{
					WordViewModel vm = wordFactory(word);
					SelectWordSegments(vm);
					return vm;
				}, vm => vm.DomainWord);
			_wordsViewModel = wordsFactory(_words);
			_newAffixCommand = new RelayCommand(NewAffix);
			_editAffixCommand = new RelayCommand(EditAffix, CanEditAffix);
			_removeAffixCommand = new RelayCommand(RemoveAffix, CanRemoveAffix);
		}

		private void variety_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "SegmentFrequencyDistribution")
			{
				Segment curSeg = null;
				if (SelectedSegment != null)
					curSeg = SelectedSegment.DomainSegment;
				_segments.ReplaceAll(DomainVariety.SegmentFrequencyDistribution.ObservedSamples.Select(seg => new VarietySegmentViewModel(this, seg)));
				MaxSegmentProbability = _segments.Select(seg => seg.Probability).Concat(0).Max();
				if (curSeg != null)
					SelectedSegment = _segments.FirstOrDefault(vm => vm.DomainSegment.Equals(curSeg));
			}
		}

		private void NewAffix()
		{
			var vm = new EditAffixViewModel(_projectService.Project.Segmenter);
			if (_dialogService.ShowModalDialog(this, vm) == true)
			{
				var affix = new Affix(vm.StrRep, vm.Type == AffixViewModelType.Prefix ? AffixType.Prefix : AffixType.Suffix, vm.Category);
				_projectService.Project.Segmenter.Segment(affix);
				DomainVariety.Affixes.Add(affix);
				Messenger.Default.Send(new DomainModelChangedMessage(false));
				SelectedAffix = _affixes.Single(a => a.DomainAffix == affix);
			}
		}

		private bool CanEditAffix()
		{
			return SelectedAffix != null;
		}

		private void EditAffix()
		{
			var vm = new EditAffixViewModel(_projectService.Project.Segmenter, _selectedAffix.DomainAffix);
			if (_dialogService.ShowModalDialog(this, vm) == true)
			{
				var affix = new Affix(vm.StrRep, vm.Type == AffixViewModelType.Prefix ? AffixType.Prefix : AffixType.Suffix, vm.Category);
				int index = DomainVariety.Affixes.IndexOf(_selectedAffix.DomainAffix);
				_projectService.Project.Segmenter.Segment(affix);
				DomainVariety.Affixes[index] = affix;
				Messenger.Default.Send(new DomainModelChangedMessage(false));
				SelectedAffix = _affixes.Single(a => a.DomainAffix == affix);
			}
		}

		private void RemoveAffix()
		{
			DomainVariety.Affixes.Remove(SelectedAffix.DomainAffix);
			Messenger.Default.Send(new DomainModelChangedMessage(false));
		}

		private bool CanRemoveAffix()
		{
			return SelectedAffix != null;
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

		public WordsViewModel Words
		{
			get { return _wordsViewModel; }
		}

		public ReadOnlyObservableList<AffixViewModel> Affixes
		{
			get { return _affixes; }
		}

		public AffixViewModel SelectedAffix
		{
			get { return _selectedAffix; }
			set { Set(() => SelectedAffix, ref _selectedAffix, value); }
		}

		public VarietySegmentViewModel SelectedSegment
		{
			get { return _selectedSegment; }
			set
			{
				if (Set(() => SelectedSegment, ref _selectedSegment, value))
				{
					_wordsViewModel.SelectedSegmentWords.Clear();
					foreach (WordViewModel word in _words)
					{
						if (SelectWordSegments(word))
							_wordsViewModel.SelectedSegmentWords.Add(word);
					}
				}
			}
		}

		private bool SelectWordSegments(WordViewModel word)
		{
			bool selected = false;
			foreach (WordSegmentViewModel segment in word.Segments.Where(s => !s.IsBoundary && !s.IsNotInOriginal))
			{
				segment.IsSelected = _selectedSegment != null && segment.DomainNode.StrRep() == _selectedSegment.StrRep;
				if (segment.IsSelected)
					selected = true;
			}
			return selected;
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
