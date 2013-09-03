using SIL.Cog.Domain.Components;

namespace SIL.Cog.Applications.ViewModels
{
	public class SimilarSegmentMappingsOptionsViewModel : ComponentOptionsViewModel
	{
		private TypeSegmentMappings _segmentMappings;

		public SimilarSegmentMappingsOptionsViewModel(ThresholdSimilarSegmentMappingsViewModel thresholdMappings, ListSimilarSegmentMappingsViewModel listMappings)
			: base("Similar segments", "Type", thresholdMappings, listMappings)
		{
		}

		public TypeSegmentMappings SegmentMappings
		{
			get { return _segmentMappings; }
			set
			{
				_segmentMappings = value;
				((ThresholdSimilarSegmentMappingsViewModel) Options[0]).SegmentMappings = value;
				((ListSimilarSegmentMappingsViewModel) Options[1]).SegmentMappings = value;
			}
		}

		public override void Setup()
		{
			int index = 0;
			if (SegmentMappings != null)
			{
				if (SegmentMappings.VowelMappings is ThresholdSegmentMappings)
					index = 0;
				else if (SegmentMappings.VowelMappings is ListSegmentMappings)
					index = 1;
			}
			SelectedOption = Options[index];
			base.Setup();
		}
	}
}
