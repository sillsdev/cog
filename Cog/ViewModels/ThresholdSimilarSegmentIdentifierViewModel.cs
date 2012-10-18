using SIL.Cog.Processors;

namespace SIL.Cog.ViewModels
{
	public class ThresholdSimilarSegmentIdentifierViewModel : ComponentSettingsViewModelBase
	{
		private int _vowelThreshold;
		private int _consThreshold;

		public ThresholdSimilarSegmentIdentifierViewModel(CogProject project)
			: base("Threshold", project)
		{
			_vowelThreshold = 500;
			_consThreshold = 600;
		}

		public ThresholdSimilarSegmentIdentifierViewModel(CogProject project, ThresholdSimilarSegmentIdentifier similarSegmentIdentifier)
			: base("Threshold", project)
		{
			_vowelThreshold = similarSegmentIdentifier.VowelThreshold;
			_consThreshold = similarSegmentIdentifier.ConsonantThreshold;
		}

		public int VowelThreshold
		{
			get { return _vowelThreshold; }
			set
			{
				Set("VowelThreshold", ref _vowelThreshold, value);
				IsChanged = true;
			}
		}

		public int ConsonantThreshold
		{
			get { return _consThreshold; }
			set
			{
				Set("ConsonantThreshold", ref _consThreshold, value);
				IsChanged = true;
			}
		}

		public override void UpdateComponent()
		{
			Project.VarietyPairProcessors["similarSegmentIdentifier"] = new ThresholdSimilarSegmentIdentifier(Project, _vowelThreshold, _consThreshold, "primary");
		}
	}
}
