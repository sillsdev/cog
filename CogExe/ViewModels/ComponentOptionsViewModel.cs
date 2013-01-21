using System.Collections.ObjectModel;

namespace SIL.Cog.ViewModels
{
	public class ComponentOptionsViewModel : ComponentSettingsViewModelBase
	{
		private readonly string _optionDisplayName;
		private readonly ReadOnlyCollection<ComponentSettingsViewModelBase> _options;
		private ComponentSettingsViewModelBase _currentOption;

		public ComponentOptionsViewModel(string displayName, string optionDisplayName, CogProject project, int selectedIndex, params ComponentSettingsViewModelBase[] options)
			: base(displayName, project)
		{
			_optionDisplayName = optionDisplayName;
			_options = new ReadOnlyCollection<ComponentSettingsViewModelBase>(options);
			foreach (ComponentSettingsViewModelBase option in _options)
				option.PropertyChanged += ChildPropertyChanged;
			_currentOption = _options[selectedIndex];
		}

		public string OptionDisplayName
		{
			get { return _optionDisplayName; }
		}

		public ComponentSettingsViewModelBase CurrentOption
		{
			get { return _currentOption; }
			set
			{
				Set(() => CurrentOption, ref _currentOption, value);
				IsChanged = true;
			}
		}

		public override void AcceptChanges()
		{
			base.AcceptChanges();
			ChildrenAcceptChanges(_options);
		}

		public ReadOnlyCollection<ComponentSettingsViewModelBase> Options
		{
			get { return _options; }
		}

		public override void UpdateComponent()
		{
			_currentOption.UpdateComponent();
		}
	}
}
