using SIL.Cog.Domain;
using SIL.Collections;

namespace SIL.Cog.Applications.ViewModels
{
	public class ComponentOptionsViewModel : ComponentSettingsViewModelBase
	{
		private readonly string _optionDisplayName;
		private readonly ReadOnlyList<ComponentSettingsViewModelBase> _options;
		private ComponentSettingsViewModelBase _currentOption;

		public ComponentOptionsViewModel(string displayName, string optionDisplayName, CogProject project, int selectedIndex, params ComponentSettingsViewModelBase[] options)
			: base(displayName, project)
		{
			_optionDisplayName = optionDisplayName;
			_options = new ReadOnlyList<ComponentSettingsViewModelBase>(options);
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
			set { SetChanged(() => CurrentOption, ref _currentOption, value); }
		}

		public override void AcceptChanges()
		{
			base.AcceptChanges();
			ChildrenAcceptChanges(_options);
		}

		public ReadOnlyList<ComponentSettingsViewModelBase> Options
		{
			get { return _options; }
		}

		public override object UpdateComponent()
		{
			return _currentOption.UpdateComponent();
		}
	}
}
