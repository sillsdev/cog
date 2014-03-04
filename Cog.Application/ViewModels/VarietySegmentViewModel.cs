using SIL.Cog.Domain;
using SIL.Machine.FeatureModel;
using SIL.Machine.Statistics;

namespace SIL.Cog.Application.ViewModels
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

			FrequencyDistribution<Segment> freqDist;
			if (position == null)
				freqDist = variety.DomainVariety.SegmentFrequencyDistribution;
			else if (!variety.DomainVariety.SyllablePositionSegmentFrequencyDistributions.TryGetValue(position, out freqDist))
				freqDist = null;

			if (freqDist != null)
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
