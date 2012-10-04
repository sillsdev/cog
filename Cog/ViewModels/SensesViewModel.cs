using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using GalaSoft.MvvmLight.Command;
using SIL.Cog.Services;

namespace SIL.Cog.ViewModels
{
	public class SensesViewModel : WorkspaceViewModelBase
	{
		private readonly IDialogService _dialogService;
		private ViewModelCollection<SenseViewModel, Sense> _senses;
		private SenseViewModel _currentSense;
		private CogProject _project;

		public SensesViewModel(IDialogService dialogService)
			: base("Senses")
		{
			_dialogService = dialogService;
			TaskAreas.Add(new TaskAreaViewModel("Common tasks", new []
				{
					new CommandViewModel("Add a new sense", new RelayCommand(AddNewSense)),
					new CommandViewModel("Edit selected sense", new RelayCommand(EditSelectedSense)), 
					new CommandViewModel("Remove selected sense", new RelayCommand(RemoveCurrentSense))
				}));
		}

		private void AddNewSense()
		{
			var vm = new EditSenseViewModel(_project);
			if (_dialogService.ShowDialog(this, vm) == true)
			{
				var newSense = new Sense(vm.Gloss, vm.Category);
				_project.Senses.Add(newSense);
				CurrentSense = _senses.Single(s => s.ModelSense == newSense);
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
			}
		}

		public override void Initialize(CogProject project)
		{
			_project = project;
			if (_senses != null)
				_senses.CollectionChanged -= SensesChanged;
			Set("Senses", ref _senses, new ViewModelCollection<SenseViewModel, Sense>(_project.Senses, sense => new SenseViewModel(sense)));
			_senses.CollectionChanged += SensesChanged;
			CurrentSense = _senses.Count > 0 ? _senses[0] : null;
		}

		private void SensesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (_currentSense == null && _senses.Count > 0)
				CurrentSense = _senses[0];
		}

		public ObservableCollection<SenseViewModel> Senses
		{
			get { return _senses; }
		}

		public SenseViewModel CurrentSense
		{
			get { return _currentSense; }
			set { Set("CurrentSense", ref _currentSense, value); }
		}
	}
}
