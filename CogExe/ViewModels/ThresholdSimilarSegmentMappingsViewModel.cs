using SIL.Cog.Components;

namespace SIL.Cog.ViewModels
{
	public class ThresholdSimilarSegmentMappingsViewModel : ComponentSettingsViewModelBase
	{
		private int _vowelThreshold;
		private int _consThreshold;

		public ThresholdSimilarSegmentMappingsViewModel(CogProject project)
			: base("Threshold", project)
		{
			_vowelThreshold = 500;
			_consThreshold = 600;
		}

		public ThresholdSimilarSegmentMappingsViewModel(CogProject project, TypeSegmentMappings similarSegmentMappings)
			: base("Threshold", project)
		{
			var vowelMappings = (ThresholdSegmentMappings) similarSegmentMappings.VowelMappings;
			_vowelThreshold = vowelMappings.Threshold;
			var consMappings = (ThresholdSegmentMappings) similarSegmentMappings.ConsonantMappings;
			_consThreshold = consMappings.Threshold;
		}

		public int VowelThreshold
		{
			get { return _vowelThreshold; }
			set
			{
				if (Set(() => VowelThreshold, ref _vowelThreshold, value))
					IsChanged = true;
			}
		}

		public int ConsonantThreshold
		{
			get { return _consThreshold; }
			set
			{
				if (Set(() => ConsonantThreshold, ref _consThreshold, value))
					IsChanged = true;
			}
		}

		public override object UpdateComponent()
		{
			return new TypeSegmentMappings(new ThresholdSegmentMappings(Project, _vowelThreshold, "primary"), new ThresholdSegmentMappings(Project, _consThreshold, "primary"));
		}
	}
}
