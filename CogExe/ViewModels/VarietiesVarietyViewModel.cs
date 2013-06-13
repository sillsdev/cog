using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using SIL.Cog.Services;
using SIL.Collections;
using SIL.Machine;

namespace SIL.Cog.ViewModels
{
	public class VarietiesVarietyViewModel : VarietyViewModel
	{
		private readonly CogProject _project;
		private readonly IDialogService _dialogService;
		private readonly UnorderedViewModelCollection<SegmentCollection, VarietySegmentViewModel, Segment> _segments;
		private readonly UnorderedViewModelCollection<ObservableCollection<Sense>, SenseViewModel, Sense> _senses;
		private readonly UnorderedViewModelCollection<WordCollection, WordViewModel, Word> _words; 
		private readonly ReadOnlyMirroredCollection<Affix, AffixViewModel> _affixes;
		private VarietySegmentViewModel _currentSegment;
		private AffixViewModel _currentAffix;
		private readonly ObservableCollection<WordViewModel> _selectedWords; 
		private readonly ICommand _newAffixCommand;
		private readonly ICommand _editAffixCommand;
		private readonly ICommand _removeAffixCommand;
 
		public VarietiesVarietyViewModel(IDialogService dialogService, CogProject project, Variety variety)
			: base(variety)
		{
			_project = project;
			_dialogService = dialogService;
			_segments = new UnorderedViewModelCollection<SegmentCollection, VarietySegmentViewModel, Segment>(variety.Segments, segment => new VarietySegmentViewModel(variety, segment), vm => vm.ModelSegment);
			_senses = new UnorderedViewModelCollection<ObservableCollection<Sense>, SenseViewModel, Sense>(_project.Senses, sense => new SenseViewModel(sense), vm => vm.ModelSense);
			_words = new UnorderedViewModelCollection<WordCollection, WordViewModel, Word>(variety.Words, word =>
				{
					var vm = new WordViewModel(project, _senses[word.Sense], word);
					vm.PropertyChanged += ChildPropertyChanged;
					return vm;
				}, vm => vm.ModelWord);
			_words.CollectionChanged += WordsChanged;

			_selectedWords = new ObservableCollection<WordViewModel>();
			_affixes = new ReadOnlyMirroredCollection<Affix, AffixViewModel>(ModelVariety.Affixes, affix => new AffixViewModel(affix));
			if (_affixes.Count > 0)
				_currentAffix = _affixes[0];
			_newAffixCommand = new RelayCommand(NewAffix);
			_editAffixCommand = new RelayCommand(EditAffix, CanEditAffix);
			_removeAffixCommand = new RelayCommand(RemoveAffix, CanRemoveAffix);
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
				Shape shape;
				if (!_project.Segmenter.ToShape(vm.StrRep, out shape))
					shape = _project.Segmenter.EmptyShape;
				var affix = new Affix(vm.StrRep, vm.Type == AffixViewModelType.Prefix ? AffixType.Prefix : AffixType.Suffix, shape, vm.Category);
				ModelVariety.Affixes.Add(affix);
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
				Shape shape;
				if (!_project.Segmenter.ToShape(vm.StrRep, out shape))
					shape = _project.Segmenter.EmptyShape;
				var affix = new Affix(vm.StrRep, vm.Type == AffixViewModelType.Prefix ? AffixType.Prefix : AffixType.Suffix, shape, vm.Category);
				int index = ModelVariety.Affixes.IndexOf(_currentAffix.ModelAffix);
				ModelVariety.Affixes[index] = affix;
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

		public ObservableCollection<VarietySegmentViewModel> Segments
		{
			get { return _segments; }
		}

		public ObservableCollection<SenseViewModel> Senses
		{
			get { return _senses; }
		}

		public ObservableCollection<WordViewModel> Words
		{
			get { return _words; }
		}

		public ReadOnlyObservableCollection<AffixViewModel> Affixes
		{
			get { return _affixes; }
		}

		public ObservableCollection<WordViewModel> SelectedWords
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
				Set(() => CurrentSegment, ref _currentSegment, value);
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
