using SIL.Cog.Application.Services;

namespace SIL.Cog.Application.ViewModels
{
	public class CompareSettingsViewModel : SettingsWorkspaceViewModelBase
	{
		public CompareSettingsViewModel(IProjectService projectService, IBusyService busyService, AlineViewModel alineViewModel,
			CognateIdentifierOptionsViewModel cognateIdentifierOptionsViewModel)
			: base("Comparison", projectService, busyService, alineViewModel, cognateIdentifierOptionsViewModel)
		{
		}
	}
}
