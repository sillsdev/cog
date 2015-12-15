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
			: base("Phonetic", "Phonetic-Method-Settings")
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
			ICognateIdentifier cognateIdentifier = _projectService.Project.CognateIdentifiers[ComponentIdentifiers.PrimaryCognateIdentifier];
			var threshold = cognateIdentifier as ThresholdCognateIdentifier;
			Set(() => Threshold, ref _threshold, threshold == null ? 0.75 : threshold.Threshold);
		}

		public override object UpdateComponent()
		{
			var cognateIdentifier = new ThresholdCognateIdentifier(_threshold);
			_projectService.Project.CognateIdentifiers[ComponentIdentifiers.PrimaryCognateIdentifier] = cognateIdentifier;
			return cognateIdentifier;
		}
	}
}
