using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;

namespace SIL.Cog.Applications.ViewModels
{
	public class EMSoundChangeInducerViewModel : ComponentSettingsViewModelBase
	{
		private readonly CogProject _project;
		private double _initialAlignmentThreshold;

		public EMSoundChangeInducerViewModel(CogProject project, EMSoundChangeInducer soundChangeInducer)
			: base("Sound correspondence induction")
		{
			_project = project;
			_initialAlignmentThreshold = soundChangeInducer.InitialAlignmentThreshold;
		}

		public double InitialAlignmentThreshold
		{
			get { return _initialAlignmentThreshold; }
			set { SetChanged(() => InitialAlignmentThreshold, ref _initialAlignmentThreshold, value); }
		}

		public override object UpdateComponent()
		{
			var soundChangeInducer = new EMSoundChangeInducer(_project, _initialAlignmentThreshold, "primary", "cognateIdentifier");
			_project.VarietyPairProcessors["soundChangeInducer"] = soundChangeInducer;
			return soundChangeInducer;
		}
	}
}
