using GalaSoft.MvvmLight;
using SIL.Machine;

namespace SIL.Cog.ViewModels
{
	public class SimilarSegmentMappingViewModel : ViewModelBase
	{
		private readonly string _segment1;
		private readonly string _segment2;
		private readonly bool _isSegment1Valid;
		private readonly bool _isSegment2Valid;

		public SimilarSegmentMappingViewModel(CogProject project, string segment1, string segment2)
		{
			_segment1 = segment1;
			_segment2 = segment2;

			Shape shape;
			_isSegment1Valid = project.Segmenter.ToShape(_segment1, out shape);
			_isSegment2Valid = project.Segmenter.ToShape(_segment2, out shape);
		}

		public string Segment1
		{
			get { return _segment1; }
		}

		public string Segment2
		{
			get { return _segment2; }
		}

		public bool IsSegment1Valid
		{
			get { return _isSegment1Valid; }
		}

		public bool IsSegment2Valid
		{
			get { return _isSegment2Valid; }
		}
	}
}
