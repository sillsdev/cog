using System.Diagnostics;
using SIL.Cog.Aligners;
using SIL.Cog.Processors;
using SIL.Cog.Services;
using SIL.Machine;

namespace SIL.Cog.ViewModels
{
	public class ComparisonSettingsViewModel : SettingsWorkspaceViewModelBase
	{
		private readonly SpanFactory<ShapeNode> _spanFactory;
		private readonly IDialogService _dialogService;

		public ComparisonSettingsViewModel(SpanFactory<ShapeNode> spanFactory, IDialogService dialogService)
		{
			_spanFactory = spanFactory;
			_dialogService = dialogService;
		}

		protected override void CreateComponents()
		{
			Components.Add(new AlineViewModel(_spanFactory, _dialogService, Project, (Aline) Project.Aligners["primary"]));
			Components.Add(new EMSoundChangeInducerViewModel(Project, (EMSoundChangeInducer) Project.VarietyPairProcessors["soundChangeInducer"]));
			IProcessor<VarietyPair> cognateIdentifier = Project.VarietyPairProcessors["cognateIdentifier"];
			ComponentSettingsViewModelBase cognateIdentifierVM = null;
			if (cognateIdentifier is BlairCognateIdentifier)
			{
				cognateIdentifierVM = new ComponentOptionsViewModel("Cognate identification", "Method", Project, 0,
					new BlairCognateIdentifierViewModel(_dialogService, Project, (BlairCognateIdentifier) cognateIdentifier),
					new ThresholdCognateIdentifierViewModel(Project));
			}
			else if (cognateIdentifier is ThresholdCognateIdentifier)
			{
				cognateIdentifierVM = new ComponentOptionsViewModel("Cognate identification", "Method", Project, 1,
					new BlairCognateIdentifierViewModel(_dialogService, Project),
					new ThresholdCognateIdentifierViewModel(Project, (ThresholdCognateIdentifier) cognateIdentifier));
			}
			Debug.Assert(cognateIdentifierVM != null);
			Components.Add(cognateIdentifierVM);
		}
	}
}
