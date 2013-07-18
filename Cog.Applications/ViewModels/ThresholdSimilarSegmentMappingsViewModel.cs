using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;

namespace SIL.Cog.Applications.ViewModels
{
	public class ThresholdSimilarSegmentMappingsViewModel : ComponentSettingsViewModelBase
	{
		private readonly CogProject _project;
		private int _vowelThreshold;
		private int _consThreshold;

		public ThresholdSimilarSegmentMappingsViewModel(CogProject project)
			: base("Threshold")
		{
			_project = project;
			_vowelThreshold = 500;
			_consThreshold = 600;
		}

		public ThresholdSimilarSegmentMappingsViewModel(CogProject project, TypeSegmentMappings similarSegmentMappings)
			: base("Threshold")
		{
			_project = project;
			var vowelMappings = (ThresholdSegmentMappings) similarSegmentMappings.VowelMappings;
			_vowelThreshold = vowelMappings.Threshold;
			var consMappings = (ThresholdSegmentMappings) similarSegmentMappings.ConsonantMappings;
			_consThreshold = consMappings.Threshold;
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

		public override object UpdateComponent()
		{
			return new TypeSegmentMappings(new ThresholdSegmentMappings(_project, _vowelThreshold, "primary"), new ThresholdSegmentMappings(_project, _consThreshold, "primary"));
		}
	}
}
