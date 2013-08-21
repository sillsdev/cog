using SIL.Cog.Applications.Services;

namespace SIL.Cog.Applications.ViewModels
{
	public class InputSettingsViewModel : SettingsWorkspaceViewModelBase
	{
		public InputSettingsViewModel(IProjectService projectService, IBusyService busyService, SspSyllabifierViewModel syllabifierViewModel,
			UnsupervisedAffixIdentifierViewModel affixIdentifierViewModel)
			: base("Input", projectService, busyService, syllabifierViewModel, affixIdentifierViewModel)
		{
		}
	}
}
