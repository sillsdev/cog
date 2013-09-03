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
		private SenseViewModel _selectedSense;

		public SensesViewModel(IProjectService projectService, IDialogService dialogService)
			: base("Senses")
		{
			_projectService = projectService;
			_dialogService = dialogService;

			_projectService.ProjectOpened += _projectService_ProjectOpened;

			TaskAreas.Add(new TaskAreaItemsViewModel("Common tasks",
				new TaskAreaCommandViewModel("Add a new sense", new RelayCommand(AddNewSense)),
				new TaskAreaCommandViewModel("Edit selected sense", new RelayCommand(EditSelectedSense)), 
				new TaskAreaCommandViewModel("Remove selected sense", new RelayCommand(RemoveSelectedSense))));
		}

		private void _projectService_ProjectOpened(object sender, EventArgs e)
		{
			Set("Senses", ref _senses, new ReadOnlyMirroredList<Sense, SenseViewModel>(_projectService.Project.Senses, sense => new SenseViewModel(sense), vm => vm.DomainSense));
			_senses.CollectionChanged += SensesChanged;
			SelectedSense = _senses.Count > 0 ? _senses[0] : null;
		}

		private void AddNewSense()
		{
			var vm = new EditSenseViewModel(_projectService.Project.Senses);
			if (_dialogService.ShowModalDialog(this, vm) == true)
			{
				var newSense = new Sense(vm.Gloss, vm.Category);
				_projectService.Project.Senses.Add(newSense);
				Messenger.Default.Send(new DomainModelChangedMessage(true));
				SelectedSense = _senses.Single(s => s.DomainSense == newSense);
			}
		}

		private void EditSelectedSense()
		{
			if (_selectedSense == null)
				return;

			var vm = new EditSenseViewModel(_projectService.Project.Senses, _selectedSense.DomainSense);
			if (_dialogService.ShowModalDialog(this, vm) == true)
			{
				_selectedSense.DomainSense.Gloss = vm.Gloss;
				_selectedSense.DomainSense.Category = vm.Category;
			}
		}

		private void RemoveSelectedSense()
		{
			if (_selectedSense == null)
				return;

			if (_dialogService.ShowYesNoQuestion(this, "Are you sure you want to remove this sense?", "Cog"))
			{
				int index = _senses.IndexOf(_selectedSense);
				_projectService.Project.Senses.Remove(_selectedSense.DomainSense);
				Messenger.Default.Send(new DomainModelChangedMessage(true));
				if (index == _senses.Count)
					index--;
				SelectedSense = _senses.Count > 0 ?  _senses[index] : null;
			}
		}

		private void SensesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (_selectedSense == null || !_senses.Contains(_selectedSense))
				SelectedSense = _senses.Count > 0 ? _senses[0] : null;
		}

		public ReadOnlyObservableList<SenseViewModel> Senses
		{
			get { return _senses; }
		}

		public SenseViewModel SelectedSense
		{
			get { return _selectedSense; }
			set { Set(() => SelectedSense, ref _selectedSense, value); }
		}
	}
}
