using SIL.Cog.Processors;

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
			set
			{
				Set(() => InitialAlignmentThreshold, ref _initialAlignmentThreshold, value);
				IsChanged = true;
			}
		}

		public override void UpdateComponent()
		{
			Project.VarietyPairProcessors["soundChangeInducer"] = new EMSoundChangeInducer(Project, _initialAlignmentThreshold, "primary", "cognateIdentifier");
		}
	}
}
