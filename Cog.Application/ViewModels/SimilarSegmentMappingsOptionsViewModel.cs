using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;

namespace SIL.Cog.Application.ViewModels
{
	public class SimilarSegmentMappingsOptionsViewModel : ComponentOptionsViewModel
	{
		public delegate SimilarSegmentMappingsOptionsViewModel Factory(SoundType soundType);

		private ISegmentMappings _segmentMappings;

		public SimilarSegmentMappingsOptionsViewModel(ThresholdSimilarSegmentMappingsViewModel.Factory thresholdMappingsFactory, ListSimilarSegmentMappingsViewModel.Factory listMappingsFactory, SoundType soundType)
			: base("Similar " + (soundType == SoundType.Vowel ? "vowels" : "consonants"), "Type", thresholdMappingsFactory(soundType), listMappingsFactory(soundType))
		{
		}

		public ISegmentMappings SegmentMappings
		{
			get { return _segmentMappings; }
			set
			{
				_segmentMappings = value;
				((ThresholdSimilarSegmentMappingsViewModel) Options[0]).SegmentMappings = _segmentMappings as ThresholdSegmentMappings;
				((ListSimilarSegmentMappingsViewModel) Options[1]).SegmentMappings = _segmentMappings as ListSegmentMappings;
			}
		}

		public override void Setup()
		{
			int index = 0;
			if (SegmentMappings != null)
			{
				if (SegmentMappings is ThresholdSegmentMappings)
					index = 0;
				else if (SegmentMappings is ListSegmentMappings)
					index = 1;
			}
			SelectedOption = Options[index];
			base.Setup();
		}
	}
}
