using SIL.Cog.Processors;

namespace SIL.Cog.ViewModels
{
	public class EMSoundChangeInducerViewModel : ComponentSettingsViewModelBase
	{
		private double _alignmentThreshold;

		public EMSoundChangeInducerViewModel(CogProject project, EMSoundChangeInducer soundChangeInducer)
			: base("Sound change induction", project)
		{
			_alignmentThreshold = soundChangeInducer.AlignmentThreshold;
		}

		public double AlignmentThreshold
		{
			get { return _alignmentThreshold; }
			set
			{
				Set(() => AlignmentThreshold, ref _alignmentThreshold, value);
				IsChanged = true;
			}
		}

		public override void UpdateComponent()
		{
			Project.VarietyPairProcessors["soundChangeInducer"] = new EMSoundChangeInducer(Project, _alignmentThreshold, "primary");
		}
	}
}
