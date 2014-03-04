namespace SIL.Cog.Application.ViewModels
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

		public abstract void Setup();

		public abstract object UpdateComponent();
	}
}
