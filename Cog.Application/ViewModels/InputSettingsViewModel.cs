using SIL.Cog.Application.Services;

namespace SIL.Cog.Application.ViewModels
{
	public class InputSettingsViewModel : SettingsWorkspaceViewModelBase
	{
		public InputSettingsViewModel(IProjectService projectService, IBusyService busyService, SyllabifierViewModel syllabifierViewModel,
			PoorMansAffixIdentifierViewModel affixIdentifierViewModel)
			: base("Input", projectService, busyService, syllabifierViewModel, affixIdentifierViewModel)
		{
		}
	}
}
