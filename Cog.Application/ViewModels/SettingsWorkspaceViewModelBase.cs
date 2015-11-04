using System;
using System.ComponentModel;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Application.Services;
using SIL.Collections;

namespace SIL.Cog.Application.ViewModels
{
	public abstract class SettingsWorkspaceViewModelBase : WorkspaceViewModelBase
	{
		private readonly IProjectService _projectService;
		private readonly ReadOnlyList<ComponentSettingsViewModelBase> _components;
		private readonly ICommand _applyCommand;
		private readonly ICommand _resetCommand;
		private bool _isDirty;
		private readonly IBusyService _busyService;
		private readonly string _title;

		protected SettingsWorkspaceViewModelBase(string title, IProjectService projectService, IBusyService busyService, params ComponentSettingsViewModelBase[] components)
			: base("Settings")
		{
			_title = title;
			_projectService = projectService;
			_busyService = busyService;

			_projectService.ProjectOpened += _projectService_ProjectOpened;

			_components = new ReadOnlyList<ComponentSettingsViewModelBase>(components);
			foreach (ComponentSettingsViewModelBase componentVM in _components)
				componentVM.PropertyChanged += Component_PropertyChanged;
			_applyCommand = new RelayCommand(Apply, CanApply);
			_resetCommand = new RelayCommand(Reset);
		}

		private void _projectService_ProjectOpened(object sender, EventArgs e)
		{
			_isDirty = true;
			Reset();
		}

		private bool CanApply()
		{
			return _isDirty;
		}

		public string Title
		{
			get { return _title; }
		}

		public void Apply()
		{
			_busyService.ShowBusyIndicatorUntilFinishDrawing();
			foreach (ComponentSettingsViewModelBase componentVM in _components)
			{
				componentVM.UpdateComponent();
				componentVM.Setup();
				componentVM.AcceptChanges();
			}
			Messenger.Default.Send(new DomainModelChangedMessage(true));
			_isDirty = false;
		}

		public void Reset()
		{
			if (!_isDirty)
				return;

			_busyService.ShowBusyIndicatorUntilFinishDrawing();
			foreach (ComponentSettingsViewModelBase componentVM in _components)
			{
				componentVM.Setup();
				componentVM.AcceptChanges();
			}
			_isDirty = false;
		}

		private void Component_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "IsChanged":
					if (((ComponentSettingsViewModelBase) sender).IsChanged)
						_isDirty = true;
					break;
			}
		}

		public bool IsDirty
		{
			get { return _isDirty; }
		}

		public ReadOnlyList<ComponentSettingsViewModelBase> Components
		{
			get { return _components; }
		}

		public ICommand ApplyCommand
		{
			get { return _applyCommand; }
		}

		public ICommand ResetCommand
		{
			get { return _resetCommand; }
		}
	}
}
