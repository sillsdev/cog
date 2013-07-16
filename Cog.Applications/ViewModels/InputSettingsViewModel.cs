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

		public InputSettingsViewModel(IBusyService busyService, SpanFactory<ShapeNode> spanFactory, IDialogService dialogService)
			: base(busyService)
		{
			_spanFactory = spanFactory;
			_dialogService = dialogService;
		}

		protected override void CreateComponents()
		{
			IProcessor<Variety> syllabifier;
			if (Project.VarietyProcessors.TryGetValue("syllabifier", out syllabifier))
				Components.Add(new SspSyllabifierViewModel(_dialogService, Project, (SspSyllabifier) syllabifier));
			else
				Components.Add(new SspSyllabifierViewModel(_dialogService, Project));
			Components.Add(new UnsupervisedAffixIdentifierViewModel(_spanFactory, Project, (UnsupervisedAffixIdentifier) Project.VarietyProcessors["affixIdentifier"]));
		}
	}
}
