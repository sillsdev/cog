using System.Collections.Specialized;
using System.Linq;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Applications.Services;
using SIL.Cog.Domain;
using SIL.Collections;

namespace SIL.Cog.Applications.ViewModels
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
			TaskAreas.Add(new TaskAreaItemsViewModel("Common tasks",
				new TaskAreaCommandViewModel("Add a new sense", new RelayCommand(AddNewSense)),
				new TaskAreaCommandViewModel("Edit selected sense", new RelayCommand(EditSelectedSense)), 
				new TaskAreaCommandViewModel("Remove selected sense", new RelayCommand(RemoveCurrentSense))));
		}

		private void AddNewSense()
		{
			var vm = new EditSenseViewModel(_project);
			if (_dialogService.ShowModalDialog(this, vm) == true)
			{
				var newSense = new Sense(vm.Gloss, vm.Category);
				Messenger.Default.Send(new DomainModelChangingMessage());
				_project.Senses.Add(newSense);
				CurrentSense = _senses.Single(s => s.DomainSense == newSense);
			}
		}

		private void EditSelectedSense()
		{
			if (_currentSense == null)
				return;

			var vm = new EditSenseViewModel(_project, _currentSense.DomainSense);
			if (_dialogService.ShowModalDialog(this, vm) == true)
			{
				_currentSense.DomainSense.Gloss = vm.Gloss;
				_currentSense.DomainSense.Category = vm.Category;
			}
		}

		private void RemoveCurrentSense()
		{
			if (_currentSense == null)
				return;

			if (_dialogService.ShowYesNoQuestion(this, "Are you sure you want to remove this sense?", "Cog"))
			{
				Messenger.Default.Send(new DomainModelChangingMessage());
				int index = _senses.IndexOf(_currentSense);
				_project.Senses.Remove(_currentSense.DomainSense);
				if (index == _senses.Count)
					index--;
				CurrentSense = _senses.Count > 0 ?  _senses[index] : null;
			}
		}

		public override void Initialize(CogProject project)
		{
			_project = project;
			Set("Senses", ref _senses, new ReadOnlyMirroredList<Sense, SenseViewModel>(_project.Senses, sense => new SenseViewModel(sense), vm => vm.DomainSense));
			_senses.CollectionChanged += SensesChanged;
			CurrentSense = _senses.Count > 0 ? _senses[0] : null;
		}

		private void SensesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (_currentSense == null || !_senses.Contains(_currentSense))
				CurrentSense = _senses.Count > 0 ? _senses[0] : null;
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
