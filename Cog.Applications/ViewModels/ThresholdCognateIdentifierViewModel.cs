using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;

namespace SIL.Cog.Applications.ViewModels
{
	public class ThresholdCognateIdentifierViewModel : ComponentSettingsViewModelBase
	{
		private double _threshold;
		private readonly CogProject _project;

		public ThresholdCognateIdentifierViewModel(CogProject project)
			: base("Phonetic")
		{
			_project = project;
			_threshold = 0.75;
		}

		public ThresholdCognateIdentifierViewModel(CogProject project, ThresholdCognateIdentifier cognateIdentifier)
			: base("Phonetic")
		{
			_project = project;
			_threshold = cognateIdentifier.Threshold;
		}

		public double Threshold
		{
			get { return _threshold; }
			set { SetChanged(() => Threshold, ref _threshold, value); }
		}

		public override object UpdateComponent()
		{
			var cognateIdentifier = new ThresholdCognateIdentifier(_project, _threshold, "primary");
			_project.VarietyPairProcessors["cognateIdentifier"] = cognateIdentifier;
			return cognateIdentifier;
		}
	}
}
