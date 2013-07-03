using System;
using SIL.Cog.Statistics;
using SIL.Collections;

namespace SIL.Cog.NgramModeling
{
	public class WittenBellSmoother : INgramModelSmoother
	{
		private ConditionalFrequencyDistribution<Tuple<Ngram, string>, Segment> _cfd; 
		private NgramModel _lowerOrderModel;
		private int _vocabularySize;
		private Direction _dir;

		public void Smooth(int ngramSize, Variety variety, Direction dir, ConditionalFrequencyDistribution<Tuple<Ngram, string>, Segment> cfd)
		{
			_cfd = cfd;
			_dir = dir;
			_vocabularySize = variety.SegmentFrequencyDistribution.ObservedSamples.Count + 1;
			if (ngramSize > 1)
				_lowerOrderModel = new NgramModel(ngramSize - 1, variety, dir, new WittenBellSmoother());
		}

		public double GetProbability(Segment seg, Ngram context, string category)
		{
			FrequencyDistribution<Segment> freqDist = _cfd[Tuple.Create(context, category)];
			double numer = freqDist[seg] + (freqDist.ObservedSamples.Count * (_lowerOrderModel == null ? 1.0 / _vocabularySize : _lowerOrderModel.GetProbability(seg, context.SkipFirst(_dir), category)));
			double denom = freqDist.SampleOutcomeCount + freqDist.ObservedSamples.Count;
			return numer / denom;
		}

		public NgramModel LowerOrderModel
		{
			get { return _lowerOrderModel; }
		}
	}
}
