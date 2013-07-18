using System.Diagnostics;
using SIL.Cog.Applications.Services;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;

namespace SIL.Cog.Applications.ViewModels
{
	public class CompareSettingsViewModel : SettingsWorkspaceViewModelBase
	{
		private readonly IDialogService _dialogService;
		private readonly IImportService _importService;

		public CompareSettingsViewModel(IProjectService projectService, IBusyService busyService, IDialogService dialogService, IImportService importService)
			: base(projectService, busyService)
		{
			_dialogService = dialogService;
			_importService = importService;
		}

		protected override void CreateComponents()
		{
			Components.Add(new AlineViewModel(_dialogService, Project, (Aline) Project.WordAligners["primary"]));
			Components.Add(new EMSoundChangeInducerViewModel(Project, (EMSoundChangeInducer) Project.VarietyPairProcessors["soundChangeInducer"]));
			IProcessor<VarietyPair> cognateIdentifier = Project.VarietyPairProcessors["cognateIdentifier"];
			ComponentSettingsViewModelBase cognateIdentifierVM = null;
			if (cognateIdentifier is BlairCognateIdentifier)
			{
				cognateIdentifierVM = new ComponentOptionsViewModel("Likely cognate identification", "Method", 0,
					new BlairCognateIdentifierViewModel(_dialogService, _importService, Project, (BlairCognateIdentifier) cognateIdentifier),
					new ThresholdCognateIdentifierViewModel(Project),
					new DolgopolskyCognateIdentifierViewModel(_dialogService, Project));
			}
			else if (cognateIdentifier is ThresholdCognateIdentifier)
			{
				cognateIdentifierVM = new ComponentOptionsViewModel("Likely cognate identification", "Method", 1,
					new BlairCognateIdentifierViewModel(_dialogService, _importService, Project),
					new ThresholdCognateIdentifierViewModel(Project, (ThresholdCognateIdentifier) cognateIdentifier),
					new DolgopolskyCognateIdentifierViewModel(_dialogService, Project));
			}
			else if (cognateIdentifier is DolgopolskyCognateIdentifier)
			{
				cognateIdentifierVM = new ComponentOptionsViewModel("Likely cognate identification", "Method", 2,
					new BlairCognateIdentifierViewModel(_dialogService, _importService, Project),
					new ThresholdCognateIdentifierViewModel(Project),
					new DolgopolskyCognateIdentifierViewModel(_dialogService, Project, (DolgopolskyCognateIdentifier) cognateIdentifier));
			}
			Debug.Assert(cognateIdentifierVM != null);
			Components.Add(cognateIdentifierVM);
		}
	}
}
