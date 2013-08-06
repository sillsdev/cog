using System;
using SIL.Cog.Domain.Statistics;
using SIL.Collections;

namespace SIL.Cog.Domain.NgramModeling
{
	public class MaxLikelihoodSmoother : INgramModelSmoother
	{
		private ConditionalFrequencyDistribution<Tuple<Ngram, string>, Segment> _cfd;

		public void Smooth(SegmentPool segmentPool, int ngramSize, Variety variety, Direction dir, ConditionalFrequencyDistribution<Tuple<Ngram, string>, Segment> cfd)
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
