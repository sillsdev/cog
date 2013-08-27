using SIL.Cog.Domain;
using SIL.Cog.Domain.Statistics;

namespace SIL.Cog.Applications.ViewModels
{
	public class VarietySegmentViewModel : SegmentViewModel
	{
		private readonly VarietyViewModel _variety;
		private readonly int _frequency;
		private readonly double _probability;

		public VarietySegmentViewModel(VarietyViewModel variety, Segment segment, SyllablePosition position)
			: base(segment)
		{
			_variety = variety;
			FrequencyDistribution<Segment> freqDist;
			if (variety.DomainVariety.SegmentFrequencyDistributions.TryGetValue(position, out freqDist))
			{
				_frequency = freqDist[segment];
				_probability = (double) _frequency / freqDist.SampleOutcomeCount;
			}
		}

		public double Probability
		{
			get { return _probability; }
		}

		public int Frequency
		{
			get { return _frequency; }
		}

		public VarietyViewModel Variety
		{
			get { return _variety; }
		}
	}
}
