using SIL.Cog.Application.Services;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;

namespace SIL.Cog.Application.ViewModels
{
	public class ThresholdCognateIdentifierViewModel : ComponentSettingsViewModelBase
	{
		private readonly IProjectService _projectService;
		private double _threshold;

		public ThresholdCognateIdentifierViewModel(IProjectService projectService)
			: base("Phonetic")
		{
			_projectService = projectService;
		}

		public double Threshold
		{
			get { return _threshold; }
			set { SetChanged(() => Threshold, ref _threshold, value); }
		}

		public override void Setup()
		{
			IProcessor<VarietyPair> cognateIdentifier = _projectService.Project.VarietyPairProcessors["cognateIdentifier"];
			var threshold = cognateIdentifier as ThresholdCognateIdentifier;
			Set(() => Threshold, ref _threshold, threshold == null ? 0.75 : threshold.Threshold);
		}

		public override object UpdateComponent()
		{
			var cognateIdentifier = new ThresholdCognateIdentifier(_projectService.Project, _threshold, "primary");
			_projectService.Project.VarietyPairProcessors["cognateIdentifier"] = cognateIdentifier;
			return cognateIdentifier;
		}
	}
}
