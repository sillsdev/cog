using System.Collections.Generic;
using SIL.Collections;

namespace SIL.Cog.Statistics
{
	public class FrequencyDistribution<TSample>
	{
		private readonly Dictionary<TSample, int> _sampleCounts;
		private int _sampleOutcomeCount;
 
		public FrequencyDistribution()
		{
			_sampleCounts = new Dictionary<TSample, int>();
		}

		public IReadOnlyCollection<TSample> ObservedSamples
		{
			get { return _sampleCounts.Keys.AsSimpleReadOnlyCollection(); }
		}

		public void Increment(TSample sample)
		{
			Increment(sample, 1);
		}

		public void Increment(TSample sample, int count)
		{
			if (count == 0)
				return;

			_sampleCounts.UpdateValue(sample, () => 0, c => c + count);
			_sampleOutcomeCount += count;
		}

		public int this[TSample sample]
		{
			get
			{
				int count;
				if (_sampleCounts.TryGetValue(sample, out count))
					return count;
				return 0;
			}
		}

		public int SampleOutcomeCount
		{
			get { return _sampleOutcomeCount; }
		}
	}
}
