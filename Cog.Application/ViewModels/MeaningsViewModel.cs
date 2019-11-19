using System;
using System.Collections.Specialized;
using System.Linq;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Application.Collections;
using SIL.Cog.Application.Services;
using SIL.Cog.Domain;
using SIL.ObjectModel;

namespace SIL.Cog.Application.ViewModels
{
	public class MeaningsViewModel : WorkspaceViewModelBase
	{
		private readonly IProjectService _projectService;
		private readonly IDialogService _dialogService;
		private readonly IBusyService _busyService;
		private MirroredBindableList<Meaning, MeaningViewModel> _meanings;
		private MeaningViewModel _selectedMeaning;

		public MeaningsViewModel(IProjectService projectService, IDialogService dialogService, IBusyService busyService)
			: base("Meanings")
		{
			_projectService = projectService;
			_dialogService = dialogService;
			_busyService = busyService;

			_projectService.ProjectOpened += _projectService_ProjectOpened;

			TaskAreas.Add(new TaskAreaItemsViewModel("Common tasks",
				new TaskAreaCommandViewModel("Add a new meaning", new RelayCommand(AddNewMeaning)),
				new TaskAreaCommandViewModel("Edit meaning", new RelayCommand(EditSelectedMeaning, CanEditSelectedMeaning)),
				new TaskAreaCommandViewModel("Remove meaning", new RelayCommand(RemoveSelectedMeaning, CanRemoveSelectedMeaning))));
			TaskAreas.Add(new TaskAreaItemsViewModel("Other tasks",
				new TaskAreaCommandViewModel("Move meaning up", new RelayCommand(MoveSelectedMeaningUp, CanMoveSelectedMeaningUp)),
				new TaskAreaCommandViewModel("Move meaning down", new RelayCommand(MoveSelectedMeaningDown, CanMoveSelectedMeaningDown))));
		}

		private void _projectService_ProjectOpened(object sender, EventArgs e)
		{
			Set("Meanings", ref _meanings, new MirroredBindableList<Meaning, MeaningViewModel>(_projectService.Project.Meanings, meaning => new MeaningViewModel(meaning), vm => vm.DomainMeaning));
			_meanings.CollectionChanged += MeaningsChanged;
			SelectedMeaning = _meanings.Count > 0 ? _meanings[0] : null;
		}

		private void AddNewMeaning()
		{
			var vm = new EditMeaningViewModel(_projectService.Project.Meanings);
			if (_dialogService.ShowModalDialog(this, vm) == true)
			{
				var newMeaning = new Meaning(vm.Gloss, vm.Category);
				_projectService.Project.Meanings.Add(newMeaning);
				Messenger.Default.Send(new DomainModelChangedMessage(true));
				SelectedMeaning = _meanings.Single(s => s.DomainMeaning == newMeaning);
			}
		}

		private bool CanEditSelectedMeaning()
		{
			return _selectedMeaning != null;
		}

		private void EditSelectedMeaning()
		{
			var vm = new EditMeaningViewModel(_projectService.Project.Meanings, _selectedMeaning.DomainMeaning);
			if (_dialogService.ShowModalDialog(this, vm) == true)
			{
				_selectedMeaning.DomainMeaning.Gloss = vm.Gloss;
				_selectedMeaning.DomainMeaning.Category = vm.Category;
				Messenger.Default.Send(new DomainModelChangedMessage(false));
			}
		}

		private bool CanRemoveSelectedMeaning()
		{
			return _selectedMeaning != null;
		}

		private void RemoveSelectedMeaning()
		{
			if (_dialogService.ShowYesNoQuestion(this, "Are you sure you want to remove this Meaning?", "Cog"))
			{
				int index = _meanings.IndexOf(_selectedMeaning);
				_projectService.Project.Meanings.Remove(_selectedMeaning.DomainMeaning);
				Messenger.Default.Send(new DomainModelChangedMessage(true));
				if (index == _meanings.Count)
					index--;
				SelectedMeaning = _meanings.Count > 0 ?  _meanings[index] : null;
			}
		}

		private void MeaningsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (_selectedMeaning == null || !_meanings.Contains(_selectedMeaning))
				SelectedMeaning = _meanings.Count > 0 ? _meanings[0] : null;
		}

		private bool CanMoveSelectedMeaningUp()
		{
			return _meanings.IndexOf(_selectedMeaning) > 0;
		}

		private void MoveSelectedMeaningUp()
		{
			_busyService.ShowBusyIndicatorUntilFinishDrawing();
			int index = _meanings.IndexOf(_selectedMeaning);
			_projectService.Project.Meanings.Move(index, index - 1);
			Messenger.Default.Send(new DomainModelChangedMessage(false));
		}

		private bool CanMoveSelectedMeaningDown()
		{
			return _meanings.IndexOf(_selectedMeaning) < _meanings.Count - 1;
		}

		private void MoveSelectedMeaningDown()
		{
			_busyService.ShowBusyIndicatorUntilFinishDrawing();
			int index = _meanings.IndexOf(_selectedMeaning);
			_projectService.Project.Meanings.Move(index, index + 1);
			Messenger.Default.Send(new DomainModelChangedMessage(false));
		}

		public ReadOnlyObservableList<MeaningViewModel> Meanings
		{
			get { return _meanings; }
		}

		public MeaningViewModel SelectedMeaning
		{
			get { return _selectedMeaning; }
			set { Set(() => SelectedMeaning, ref _selectedMeaning, value); }
		}
	}
}
