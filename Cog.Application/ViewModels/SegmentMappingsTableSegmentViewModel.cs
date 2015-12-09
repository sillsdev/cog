using SIL.Cog.Application.Collections;
using SIL.Cog.Domain;
using SIL.Collections;

namespace SIL.Cog.Application.ViewModels
{
	public class SegmentMappingsTableSegmentViewModel : SegmentViewModel
	{
		private readonly BindableList<SegmentMappingsTableSegmentPairViewModel> _segmentPairs;

		public SegmentMappingsTableSegmentViewModel(Segment segment, SoundType type)
			: base(segment, type)
		{
			_segmentPairs = new BindableList<SegmentMappingsTableSegmentPairViewModel>();
		}

		public ObservableList<SegmentMappingsTableSegmentPairViewModel> SegmentPairs
		{
			get { return _segmentPairs; }
		}
	}
}
