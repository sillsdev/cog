using SIL.Cog.Application.Services;
using SIL.Cog.Domain.Components;

namespace SIL.Cog.Application.ViewModels
{
	public class ThresholdSimilarSegmentMappingsViewModel : ComponentSettingsViewModelBase
	{
		public delegate ThresholdSimilarSegmentMappingsViewModel Factory(SoundType soundType);

		private readonly IProjectService _projectService;
		private int _threshold;
		private readonly SoundType _soundType;

		public ThresholdSimilarSegmentMappingsViewModel(IProjectService projectService, SoundType soundType)
			: base("Threshold")
		{
			_projectService = projectService;
			_soundType = soundType;
		}

		public int Threshold
		{
			get { return _threshold; }
			set { SetChanged(() => Threshold, ref _threshold, value); }
		}

		public ThresholdSegmentMappings SegmentMappings { get; set; }

		public override void Setup()
		{
			if (SegmentMappings == null)
				Set(() => Threshold, ref _threshold, _soundType == SoundType.Vowel ? 500 : 600);
			else
				Set(() => Threshold, ref _threshold, SegmentMappings.Threshold);
		}

		public override object UpdateComponent()
		{
			return new ThresholdSegmentMappings(_projectService.Project, _threshold, "primary");
		}
	}
}
