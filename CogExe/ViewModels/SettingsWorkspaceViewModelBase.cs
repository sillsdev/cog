using System.ComponentModel;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using SIL.Collections;

namespace SIL.Cog.ViewModels
{
	public abstract class SettingsWorkspaceViewModelBase : WorkspaceViewModelBase
	{
		private CogProject _project;
		private readonly ObservableList<ComponentSettingsViewModelBase> _components;
		private readonly ICommand _applyCommand;
		private readonly ICommand _resetCommand;
		private bool _isDirty;

		protected SettingsWorkspaceViewModelBase() 
			: base("Settings")
		{
			_components = new ObservableList<ComponentSettingsViewModelBase>();
			_applyCommand = new RelayCommand(Apply, CanApply);
			_resetCommand = new RelayCommand(Reset);
		}

		public override void Initialize(CogProject project)
		{
			_project = project;
			_isDirty = true;
			Reset();
		}

		protected CogProject Project
		{
			get { return _project; }
		}

		private bool CanApply()
		{
			return _isDirty;
		}

		public void Apply()
		{
			foreach (ComponentSettingsViewModelBase componentVM in _components)
			{
				componentVM.UpdateComponent();
				componentVM.AcceptChanges();
			}
			IsChanged = true;
			_isDirty = false;
		}

		public void Reset()
		{
			if (!_isDirty)
				return;

			_components.Clear();
			CreateComponents();
			foreach (ComponentSettingsViewModelBase componentVM in _components)
				componentVM.PropertyChanged += Component_PropertyChanged;

			_isDirty = false;
		}

		protected abstract void CreateComponents();

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

		public ObservableList<ComponentSettingsViewModelBase> Components
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
