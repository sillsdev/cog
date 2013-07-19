using SIL.Cog.Applications.Services;
using SIL.Cog.Domain.Components;

namespace SIL.Cog.Applications.ViewModels
{
	public class ThresholdSimilarSegmentMappingsViewModel : ComponentSettingsViewModelBase
	{
		private readonly IProjectService _projectService;
		private int _vowelThreshold;
		private int _consThreshold;

		public ThresholdSimilarSegmentMappingsViewModel(IProjectService projectService)
			: base("Threshold")
		{
			_projectService = projectService;
		}

		public int VowelThreshold
		{
			get { return _vowelThreshold; }
			set { SetChanged(() => VowelThreshold, ref _vowelThreshold, value); }
		}

		public int ConsonantThreshold
		{
			get { return _consThreshold; }
			set { SetChanged(() => ConsonantThreshold, ref _consThreshold, value); }
		}

		public TypeSegmentMappings SegmentMappings { get; set; }

		public override void Setup()
		{
			if (SegmentMappings == null || !(SegmentMappings.VowelMappings is ThresholdSegmentMappings))
			{
				Set(() => VowelThreshold, ref _vowelThreshold, 500);
				Set(() => ConsonantThreshold, ref _consThreshold, 600);
			}
			else
			{
				var vowelMappings = (ThresholdSegmentMappings) SegmentMappings.VowelMappings;
				Set(() => VowelThreshold, ref _vowelThreshold, vowelMappings.Threshold);
				var consMappings = (ThresholdSegmentMappings) SegmentMappings.ConsonantMappings;
				Set(() => ConsonantThreshold, ref _consThreshold, consMappings.Threshold);
			}
		}

		public override object UpdateComponent()
		{
			return new TypeSegmentMappings(new ThresholdSegmentMappings(_projectService.Project, _vowelThreshold, "primary"),
				new ThresholdSegmentMappings(_projectService.Project, _consThreshold, "primary"));
		}
	}
}
