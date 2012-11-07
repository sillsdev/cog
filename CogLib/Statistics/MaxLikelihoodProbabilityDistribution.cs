namespace SIL.Cog.Statistics
{
	public class MaxLikelihoodProbabilityDistribution<TSample> : IProbabilityDistribution<TSample>
	{
		private readonly FrequencyDistribution<TSample> _freqDist; 

		public MaxLikelihoodProbabilityDistribution(FrequencyDistribution<TSample> freqDist)
		{
			_freqDist = freqDist;
		}

		public double GetProbability(TSample sample)
		{
			if (_freqDist.SampleOutcomeCount == 0)
				return 0;
			return (double) _freqDist[sample] / _freqDist.SampleOutcomeCount;
		}

		public FrequencyDistribution<TSample> FrequencyDistribution
		{
			get { return _freqDist; }
		}
	}
}
