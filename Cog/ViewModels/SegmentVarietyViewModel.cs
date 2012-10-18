using GalaSoft.MvvmLight;

namespace SIL.Cog.ViewModels
{
	public class SegmentVarietyViewModel : ViewModelBase
	{
		private readonly Segment _segment;

		public SegmentVarietyViewModel(Segment segment)
		{
			_segment = segment;
		}

		public string StrRep
		{
			get { return _segment.NormalizedStrRep; }
		}

		public double Probability
		{
			get { return _segment.Probability; }
		}

		public int Frequency
		{
			get { return _segment.Frequency; }
		}

		public Segment ModelSegment
		{
			get { return _segment; }
		}
	}
}
