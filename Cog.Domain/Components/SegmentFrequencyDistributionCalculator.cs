using System.Collections.Generic;
using System.Linq;
using SIL.Collections;
using SIL.Machine.Annotations;
using SIL.Machine.FeatureModel;
using SIL.Machine.Statistics;

namespace SIL.Cog.Domain.Components
{
	public class SegmentFrequencyDistributionCalculator : IProcessor<Variety>
	{
		private readonly SegmentPool _segmentPool;

		public SegmentFrequencyDistributionCalculator(SegmentPool segmentPool)
		{
			_segmentPool = segmentPool;
		}

		public void Process(Variety data)
		{
			var posFreqDists = new Dictionary<FeatureSymbol, FrequencyDistribution<Segment>>
				{
					{CogFeatureSystem.Onset, new FrequencyDistribution<Segment>()},
					{CogFeatureSystem.Nucleus, new FrequencyDistribution<Segment>()},
					{CogFeatureSystem.Coda, new FrequencyDistribution<Segment>()}
				};

			var freqDist = new FrequencyDistribution<Segment>();

			foreach (Word word in data.Words)
			{
				foreach (ShapeNode node in word.Shape.Where(n => n.Type().IsOneOf(CogFeatureSystem.VowelType, CogFeatureSystem.ConsonantType)))
				{
					Segment seg = _segmentPool.Get(node);
					SymbolicFeatureValue pos;
					if (node.Annotation.FeatureStruct.TryGetValue(CogFeatureSystem.SyllablePosition, out pos))
						posFreqDists[(FeatureSymbol) pos].Increment(seg);
					freqDist.Increment(seg);
				}
			}

			foreach (KeyValuePair<FeatureSymbol, FrequencyDistribution<Segment>> kvp in posFreqDists)
				data.SyllablePositionSegmentFrequencyDistributions[kvp.Key] = kvp.Value;

			data.SegmentFrequencyDistribution = freqDist;
		}
	}
}
