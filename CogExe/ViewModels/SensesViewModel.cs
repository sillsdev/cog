using System.Collections.Specialized;
using System.Linq;
using GalaSoft.MvvmLight.Command;
using SIL.Cog.Services;
using SIL.Collections;

namespace SIL.Cog.ViewModels
{
	public class SensesViewModel : WorkspaceViewModelBase
	{
		private readonly IDialogService _dialogService;
		private ReadOnlyMirroredList<Sense, SenseViewModel> _senses;
		private SenseViewModel _currentSense;
		private CogProject _project;

		public SensesViewModel(IDialogService dialogService)
			: base("Senses")
		{
			_dialogService = dialogService;
			TaskAreas.Add(new TaskAreaCommandsViewModel("Common tasks",
				new CommandViewModel("Add a new sense", new RelayCommand(AddNewSense)),
				new CommandViewModel("Edit selected sense", new RelayCommand(EditSelectedSense)), 
				new CommandViewModel("Remove selected sense", new RelayCommand(RemoveCurrentSense))));
		}

		private void AddNewSense()
		{
			var vm = new EditSenseViewModel(_project);
			if (_dialogService.ShowDialog(this, vm) == true)
			{
				var newSense = new Sense(vm.Gloss, vm.Category);
				_project.Senses.Add(newSense);
				CurrentSense = _senses.Single(s => s.ModelSense == newSense);
				IsChanged = true;
			}
		}

		private void EditSelectedSense()
		{
			if (_currentSense == null)
				return;

			var vm = new EditSenseViewModel(_project, _currentSense.ModelSense);
			if (_dialogService.ShowDialog(this, vm) == true)
			{
				_currentSense.ModelSense.Gloss = vm.Gloss;
				_currentSense.ModelSense.Category = vm.Category;
				IsChanged = true;
			}
		}

		private void RemoveCurrentSense()
		{
			if (_currentSense == null)
				return;

			if (_dialogService.ShowYesNoQuestion(this, "Are you sure you want to remove this sense?", null))
			{
				int index = _senses.IndexOf(_currentSense);
				_project.Senses.Remove(_currentSense.ModelSense);
				if (index == _senses.Count)
					index--;
				CurrentSense = _senses.Count > 0 ?  _senses[index] : null;
				IsChanged = true;
			}
		}

		public override void Initialize(CogProject project)
		{
			_project = project;
			Set("Senses", ref _senses, new ReadOnlyMirroredList<Sense, SenseViewModel>(_project.Senses, sense => new SenseViewModel(sense), vm => vm.ModelSense));
			_senses.CollectionChanged += SensesChanged;
			CurrentSense = _senses.Count > 0 ? _senses[0] : null;
		}

		private void SensesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (_currentSense == null && _senses.Count > 0)
				CurrentSense = _senses[0];
		}

		public ReadOnlyObservableList<SenseViewModel> Senses
		{
			get { return _senses; }
		}

		public SenseViewModel CurrentSense
		{
			get { return _currentSense; }
			set { Set(() => CurrentSense, ref _currentSense, value); }
		}
	}
}
