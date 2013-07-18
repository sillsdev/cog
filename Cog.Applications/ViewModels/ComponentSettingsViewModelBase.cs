namespace SIL.Cog.Applications.ViewModels
{
	public abstract class ComponentSettingsViewModelBase : ChangeTrackingViewModelBase
	{
		private readonly string _displayName;

		protected ComponentSettingsViewModelBase(string displayName)
		{
			_displayName = displayName;
		}

		public string DisplayName
		{
			get { return _displayName; }
		}

		public abstract object UpdateComponent();
	}
}
