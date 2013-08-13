using SIL.Cog.Domain;
using SIL.Cog.Domain.Statistics;

namespace SIL.Cog.Applications.ViewModels
{
	public class VarietySegmentViewModel : SegmentViewModel
	{
		private readonly int _frequency;
		private readonly double _probability;

		public VarietySegmentViewModel(Variety variety, Segment segment, SyllablePosition position)
			: base(segment)
		{
			FrequencyDistribution<Segment> freqDist = variety.SegmentFrequencyDistributions[position];
			_frequency = freqDist[segment];
			_probability = (double) _frequency / freqDist.SampleOutcomeCount;
		}

		public double Probability
		{
			get { return _probability; }
		}

		public int Frequency
		{
			get { return _frequency; }
		}
	}
}
