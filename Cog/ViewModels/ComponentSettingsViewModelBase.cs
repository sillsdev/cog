namespace SIL.Cog.ViewModels
{
	public abstract class ComponentSettingsViewModelBase : CogViewModelBase
	{
		private readonly CogProject _project;

		protected ComponentSettingsViewModelBase(string displayName, CogProject project)
			: base(displayName)
		{
			_project = project;
		}

		protected CogProject Project
		{
			get { return _project; }
		}

		public abstract void UpdateComponent();
	}
}
