using SIL.Cog.Applications.Services;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;
using SIL.Machine;

namespace SIL.Cog.Applications.ViewModels
{
	public class InputSettingsViewModel : SettingsWorkspaceViewModelBase
	{
		private readonly SpanFactory<ShapeNode> _spanFactory;
		private readonly IDialogService _dialogService;
		private readonly IAnalysisService _analysisService;

		public InputSettingsViewModel(SpanFactory<ShapeNode> spanFactory, IProjectService projectService, IBusyService busyService, IDialogService dialogService, IAnalysisService analysisService)
			: base(projectService, busyService)
		{
			_spanFactory = spanFactory;
			_dialogService = dialogService;
			_analysisService = analysisService;
		}

		protected override void CreateComponents()
		{
			IProcessor<Variety> syllabifier;
			if (Project.VarietyProcessors.TryGetValue("syllabifier", out syllabifier))
				Components.Add(new SspSyllabifierViewModel(_dialogService, _analysisService, Project, (SspSyllabifier) syllabifier));
			else
				Components.Add(new SspSyllabifierViewModel(_dialogService, _analysisService, Project));
			Components.Add(new UnsupervisedAffixIdentifierViewModel(_spanFactory, Project, (UnsupervisedAffixIdentifier) Project.VarietyProcessors["affixIdentifier"]));
		}
	}
}
