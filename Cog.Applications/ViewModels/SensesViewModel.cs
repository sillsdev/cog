using System;
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
		private readonly IProjectService _projectService;
		private readonly IDialogService _dialogService;
		private ReadOnlyMirroredList<Sense, SenseViewModel> _senses;
		private SenseViewModel _currentSense;

		public SensesViewModel(IProjectService projectService, IDialogService dialogService)
			: base("Senses")
		{
			_projectService = projectService;
			_dialogService = dialogService;

			_projectService.ProjectOpened += _projectService_ProjectOpened;

			TaskAreas.Add(new TaskAreaItemsViewModel("Common tasks",
				new TaskAreaCommandViewModel("Add a new sense", new RelayCommand(AddNewSense)),
				new TaskAreaCommandViewModel("Edit selected sense", new RelayCommand(EditSelectedSense)), 
				new TaskAreaCommandViewModel("Remove selected sense", new RelayCommand(RemoveCurrentSense))));
		}

		private void _projectService_ProjectOpened(object sender, EventArgs e)
		{
			Set("Senses", ref _senses, new ReadOnlyMirroredList<Sense, SenseViewModel>(_projectService.Project.Senses, sense => new SenseViewModel(sense), vm => vm.DomainSense));
			_senses.CollectionChanged += SensesChanged;
			CurrentSense = _senses.Count > 0 ? _senses[0] : null;
		}

		private void AddNewSense()
		{
			var vm = new EditSenseViewModel(_projectService.Project.Senses);
			if (_dialogService.ShowModalDialog(this, vm) == true)
			{
				var newSense = new Sense(vm.Gloss, vm.Category);
				_projectService.Project.Senses.Add(newSense);
				Messenger.Default.Send(new DomainModelChangedMessage(true));
				CurrentSense = _senses.Single(s => s.DomainSense == newSense);
			}
		}

		private void EditSelectedSense()
		{
			if (_currentSense == null)
				return;

			var vm = new EditSenseViewModel(_projectService.Project.Senses, _currentSense.DomainSense);
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
				int index = _senses.IndexOf(_currentSense);
				_projectService.Project.Senses.Remove(_currentSense.DomainSense);
				Messenger.Default.Send(new DomainModelChangedMessage(true));
				if (index == _senses.Count)
					index--;
				CurrentSense = _senses.Count > 0 ?  _senses[index] : null;
			}
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
