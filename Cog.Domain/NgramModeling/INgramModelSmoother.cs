using System;
using SIL.Cog.Domain.Statistics;
using SIL.Collections;

namespace SIL.Cog.Domain.NgramModeling
{
	public interface INgramModelSmoother
	{
		void Smooth(int ngramSize, Variety variety, Direction dir, ConditionalFrequencyDistribution<Tuple<Ngram, string>, Segment> cfd);

		double GetProbability(Segment seg, Ngram context, string category);

		NgramModel LowerOrderModel { get; }
	}
}
