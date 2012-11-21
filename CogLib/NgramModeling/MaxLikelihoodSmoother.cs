using System;
using SIL.Cog.Statistics;
using SIL.Collections;

namespace SIL.Cog.NgramModeling
{
	public class MaxLikelihoodSmoother : INgramModelSmoother
	{
		private ConditionalFrequencyDistribution<Tuple<Ngram, string>, Segment> _cfd;

		public void Smooth(int ngramSize, Variety variety, Direction dir, ConditionalFrequencyDistribution<Tuple<Ngram, string>, Segment> cfd)
		{
			_cfd = cfd;
		}

		public double GetProbability(Segment seg, Ngram context, string category)
		{
			FrequencyDistribution<Segment> fd = _cfd[Tuple.Create(context, category)];
			if (fd.SampleOutcomeCount == 0)
				return 0;
			return (double) fd[seg] / fd.SampleOutcomeCount;
		}

		public NgramModel LowerOrderModel
		{
			get { return null; }
		}
	}
}
