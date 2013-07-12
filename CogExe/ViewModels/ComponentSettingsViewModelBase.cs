namespace SIL.Cog.ViewModels
{
	public abstract class ComponentSettingsViewModelBase : ChangeTrackingViewModelBase
	{
		private readonly CogProject _project;
		private readonly string _displayName;

		protected ComponentSettingsViewModelBase(string displayName, CogProject project)
		{
			_displayName = displayName;
			_project = project;
		}

		protected CogProject Project
		{
			get { return _project; }
		}

		public string DisplayName
		{
			get { return _displayName; }
		}

		public abstract object UpdateComponent();
	}
}
