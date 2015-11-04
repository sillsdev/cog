using SIL.Cog.Application.Collections;
using SIL.Cog.Domain;
using SIL.Collections;

namespace SIL.Cog.Application.ViewModels
{
	public class SegmentMappingsChartSegmentViewModel : SegmentViewModel
	{
		private readonly BindableList<SegmentMappingsChartSegmentPairViewModel> _segmentPairs;

		public SegmentMappingsChartSegmentViewModel(Segment segment, SoundType type)
			: base(segment, type)
		{
			_segmentPairs = new BindableList<SegmentMappingsChartSegmentPairViewModel>();
		}

		public ObservableList<SegmentMappingsChartSegmentPairViewModel> SegmentPairs
		{
			get { return _segmentPairs; }
		}
	}
}
