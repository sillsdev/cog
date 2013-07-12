using SIL.Cog.Components;

namespace SIL.Cog.ViewModels
{
	public class EMSoundChangeInducerViewModel : ComponentSettingsViewModelBase
	{
		private double _initialAlignmentThreshold;

		public EMSoundChangeInducerViewModel(CogProject project, EMSoundChangeInducer soundChangeInducer)
			: base("Sound correspondence induction", project)
		{
			_initialAlignmentThreshold = soundChangeInducer.InitialAlignmentThreshold;
		}

		public double InitialAlignmentThreshold
		{
			get { return _initialAlignmentThreshold; }
			set { SetChanged(() => InitialAlignmentThreshold, ref _initialAlignmentThreshold, value); }
		}

		public override object UpdateComponent()
		{
			var soundChangeInducer = new EMSoundChangeInducer(Project, _initialAlignmentThreshold, "primary", "cognateIdentifier");
			Project.VarietyPairProcessors["soundChangeInducer"] = soundChangeInducer;
			return soundChangeInducer;
		}
	}
}
