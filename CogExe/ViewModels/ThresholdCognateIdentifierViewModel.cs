using SIL.Cog.Components;

namespace SIL.Cog.ViewModels
{
	public class ThresholdCognateIdentifierViewModel : ComponentSettingsViewModelBase
	{
		private double _threshold;

		public ThresholdCognateIdentifierViewModel(CogProject project)
			: base("Phonetic", project)
		{
			_threshold = 0.75;
		}

		public ThresholdCognateIdentifierViewModel(CogProject project, ThresholdCognateIdentifier cognateIdentifier)
			: base("Phonetic", project)
		{
			_threshold = cognateIdentifier.Threshold;
		}

		public double Threshold
		{
			get { return _threshold; }
			set
			{
				Set(() => Threshold, ref _threshold, value);
				IsChanged = true;
			}
		}

		public override object UpdateComponent()
		{
			var cognateIdentifier = new ThresholdCognateIdentifier(Project, _threshold, "primary");
			Project.VarietyPairProcessors["cognateIdentifier"] = cognateIdentifier;
			return cognateIdentifier;
		}
	}
}
