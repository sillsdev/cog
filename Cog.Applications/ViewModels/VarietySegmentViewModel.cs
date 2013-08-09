using SIL.Cog.Domain;

namespace SIL.Cog.Applications.ViewModels
{
	public class VarietySegmentViewModel : SegmentViewModel
	{
		private readonly Variety _variety;

		public VarietySegmentViewModel(Variety variety, Segment segment)
			: base(segment)
		{
			_variety = variety;
		}

		public double Probability
		{
			get { return _variety.SegmentProbabilityDistribution[DomainSegment]; }
		}

		public int Frequency
		{
			get { return _variety.SegmentFrequencyDistribution[DomainSegment]; }
		}
	}
}
