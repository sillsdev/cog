using SIL.Cog.Domain;
using SIL.Machine.FeatureModel;
using SIL.Machine.Statistics;

namespace SIL.Cog.Applications.ViewModels
{
	public class VarietySegmentViewModel : SegmentViewModel
	{
		private readonly VarietyViewModel _variety;
		private readonly int _frequency;
		private readonly double _probability;

		public VarietySegmentViewModel(VarietyViewModel variety, Segment segment)
			: this(variety, segment, null)
		{
		}

		public VarietySegmentViewModel(VarietyViewModel variety, Segment segment, FeatureSymbol position)
			: base(segment)
		{
			_variety = variety;
			FrequencyDistribution<Segment> freqDist = position == null ? variety.DomainVariety.SegmentFrequencyDistribution
				: variety.DomainVariety.SyllablePositionSegmentFrequencyDistributions[position];

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

		public VarietyViewModel Variety
		{
			get { return _variety; }
		}
	}
}
