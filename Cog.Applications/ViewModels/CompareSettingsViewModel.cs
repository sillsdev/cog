using SIL.Cog.Applications.Services;

namespace SIL.Cog.Applications.ViewModels
{
	public class CompareSettingsViewModel : SettingsWorkspaceViewModelBase
	{
		public CompareSettingsViewModel(IProjectService projectService, IBusyService busyService, AlineViewModel alineViewModel,
			EMSoundChangeInducerViewModel soundChangeInducerViewModel, CognateIdentifierOptionsViewModel cognateIdentifierOptionsViewModel)
			: base(projectService, busyService, alineViewModel, soundChangeInducerViewModel, cognateIdentifierOptionsViewModel)
		{
		}
	}
}
