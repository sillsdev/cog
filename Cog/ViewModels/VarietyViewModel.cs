using System.Collections.ObjectModel;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using SIL.Cog.Services;
using SIL.Machine;

namespace SIL.Cog.ViewModels
{
	public class VarietyViewModel : WrapperViewModel
	{
		private readonly CogProject _project;
		private readonly Variety _variety;
		private readonly IDialogService _dialogService;
		private readonly SegmentVarietyViewModelCollection _segments;
		private readonly VarietySenseViewModelCollection<VarietySenseViewModel> _senses;
		private readonly ViewModelCollection<AffixViewModel, Affix> _affixes;
		private AffixViewModel _currentAffix;
		private readonly ICommand _newAffixCommand;
		private readonly ICommand _removeAffixCommand;
 
		public VarietyViewModel(IDialogService dialogService, CogProject project, Variety variety)
			: base(variety)
		{
			_project = project;
			_variety = variety;
			_dialogService = dialogService;
			_segments = new SegmentVarietyViewModelCollection(_variety.Segments);
			_senses = new VarietySenseViewModelCollection<VarietySenseViewModel>(_project.Senses, _variety.Words, sense => new VarietySenseViewModel(sense, _variety.Words[sense]));
			_affixes = new ViewModelCollection<AffixViewModel, Affix>(_variety.Affixes, affix => new AffixViewModel(affix));
			if (_affixes.Count > 0)
				_currentAffix = _affixes[0];
			_newAffixCommand = new RelayCommand(ExecuteNewAffix);
			_removeAffixCommand = new RelayCommand(ExecuteRemoveAffix, CanExecuteRemoveAffix);
		}

		private void ExecuteNewAffix()
		{
			var vm = new NewAffixViewModel(_project);
			if (_dialogService.ShowDialog(this, vm) == true)
			{
				Shape shape;
				if (!_project.Segmenter.ToShape(vm.StrRep, out shape))
					shape = _project.Segmenter.EmptyShape;
				_variety.Affixes.Add(new Affix(vm.StrRep, vm.Type == "Prefix" ? AffixType.Prefix : AffixType.Suffix, shape, vm.Category));
			}
		}

		private void ExecuteRemoveAffix()
		{
			_variety.Affixes.Remove(CurrentAffix.ModelAffix);
		}

		private bool CanExecuteRemoveAffix()
		{
			return CurrentAffix != null;
		}

		public Variety ModelVariety
		{
			get { return _variety; }
		}

		public string Name
		{
			get { return _variety.Name; }
			set { _variety.Name = value; }
		}

		public ObservableCollection<SegmentVarietyViewModel> Segments
		{
			get { return _segments; }
		}

		public ObservableCollection<VarietySenseViewModel> Senses
		{
			get { return _senses; }
		}

		public ObservableCollection<AffixViewModel> Affixes
		{
			get { return _affixes; }
		}

		public AffixViewModel CurrentAffix
		{
			get { return _currentAffix; }
			set { Set("CurrentAffix", ref _currentAffix, value); }
		}

		public ICommand NewAffixCommand
		{
			get { return _newAffixCommand; }
		}

		public ICommand RemoveAffixCommand
		{
			get { return _removeAffixCommand; }
		}
	}
}
