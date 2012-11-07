using System;
using System.Linq;
using SIL.Cog.Statistics;

namespace SIL.Cog.NgramModeling
{
	public class WittenBellSmoother : INgramModelSmoother
	{
		private ConditionalFrequencyDistribution<Tuple<Ngram, string>, Segment> _cfd; 
		private NgramModel _lowerOrderModel;
		private int _vocabularySize;

		public void Smooth(NgramModel model, ConditionalFrequencyDistribution<Tuple<Ngram, string>, Segment> cfd)
		{
			_cfd = cfd;
			_vocabularySize = model.Variety.Segments.Count + 1;
			if (model.NgramSize > 1)
				_lowerOrderModel = NgramModel.Build(model.NgramSize - 1, model.Variety, model.Direction, new WittenBellSmoother());
		}

		public double GetProbability(Segment seg, Ngram context, string category)
		{
			FrequencyDistribution<Segment> freqDist = _cfd[Tuple.Create(context, category)];
			double numer = freqDist[seg] + (freqDist.ObservedSamples.Count * (_lowerOrderModel == null ? 1.0 / _vocabularySize : _lowerOrderModel.GetProbability(seg, new Ngram(context.Skip(1)), category)));
			double denom = freqDist.SampleOutcomeCount + freqDist.ObservedSamples.Count;
			return numer / denom;
		}

		public NgramModel LowerOrderModel
		{
			get { return _lowerOrderModel; }
		}
	}
}
