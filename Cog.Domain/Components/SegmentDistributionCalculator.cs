using System.Linq;
using SIL.Cog.Domain.Statistics;
using SIL.Collections;
using SIL.Machine;

namespace SIL.Cog.Domain.Components
{
	public class SegmentDistributionCalculator : IProcessor<Variety>
	{
		private readonly SegmentPool _segmentPool;

		public SegmentDistributionCalculator(SegmentPool segmentPool)
		{
			_segmentPool = segmentPool;
		}

		public void Process(Variety data)
		{
			var segFreqDist = new FrequencyDistribution<Segment>();
			foreach (Word word in data.Words)
			{
				foreach (ShapeNode node in word.Shape.Where(n => n.Type().IsOneOf(CogFeatureSystem.VowelType, CogFeatureSystem.ConsonantType)))
					segFreqDist.Increment(_segmentPool.Get(node));
			}
			data.SegmentFrequencyDistribution = segFreqDist;
			data.SegmentProbabilityDistribution = new MaxLikelihoodProbabilityDistribution<Segment>(segFreqDist);
		}
	}
}
