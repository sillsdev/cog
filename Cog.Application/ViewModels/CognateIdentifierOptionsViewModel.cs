using SIL.Cog.Application.Services;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;

namespace SIL.Cog.Application.ViewModels
{
	public class CognateIdentifierOptionsViewModel : ComponentOptionsViewModel
	{
		private readonly IProjectService _projectService;

		public CognateIdentifierOptionsViewModel(IProjectService projectService, BlairCognateIdentifierViewModel blairCognateIdentifier, ThresholdCognateIdentifierViewModel thresholdCognateIdentifier,
			DolgopolskyCognateIdentifierViewModel dolgopolskyCognateIdentifier)
			: base("Likely cognate identification", "Method", blairCognateIdentifier, thresholdCognateIdentifier, dolgopolskyCognateIdentifier)
		{
			_projectService = projectService;
		}

		public override void Setup()
		{
			IProcessor<VarietyPair> cognateIdentifier = _projectService.Project.VarietyPairProcessors["cognateIdentifier"];
			int index = -1;
			if (cognateIdentifier is BlairCognateIdentifier)
				index = 0;
			else if (cognateIdentifier is ThresholdCognateIdentifier)
				index = 1;
			else if (cognateIdentifier is DolgopolskyCognateIdentifier)
				index = 2;
			SelectedOption = Options[index];
			base.Setup();
		}
	}
}
