using System;
using SIL.Cog.Statistics;

namespace SIL.Cog.NgramModeling
{
	public interface INgramModelSmoother
	{
		void Smooth(NgramModel model, ConditionalFrequencyDistribution<Tuple<Ngram, string>, Segment> cfd);

		double GetProbability(Segment seg, Ngram context, string category);

		NgramModel LowerOrderModel { get; }
	}
}
