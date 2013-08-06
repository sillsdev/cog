using System;
using System.Collections.Generic;
using SIL.Cog.Domain.Statistics;
using SIL.Collections;

namespace SIL.Cog.Domain.NgramModeling
{
	public class ModifiedKneserNeySmoother : INgramModelSmoother
	{
		private readonly Dictionary<string, Tuple<double, double, double>> _discounts;
		private readonly Dictionary<Tuple<Ngram, string>, Tuple<int, int, int>> _bigNs;
		private ConditionalFrequencyDistribution<Tuple<Ngram, string>, Segment> _cfd;
		private NgramModel _lowerOrderModel;
		private Direction _dir;

		public ModifiedKneserNeySmoother()
		{
			_bigNs = new Dictionary<Tuple<Ngram, string>, Tuple<int, int, int>>();
			_discounts = new Dictionary<string, Tuple<double, double, double>>();
		}

		public void Smooth(SegmentPool segmentPool, int ngramSize, Variety variety, Direction dir, ConditionalFrequencyDistribution<Tuple<Ngram, string>, Segment> cfd)
		{
			_cfd = cfd;
			_dir = dir;
			var ns = new Dictionary<string, Tuple<int, int, int, int>>();
			_bigNs.Clear();
			foreach (Tuple<Ngram, string> cond in cfd.Conditions)
			{
				int n1 = 0, n2 = 0, n3 = 0, n4 = 0;
				int nGreater = 0;
				FrequencyDistribution<Segment> freqDist = cfd[cond];
				foreach (Segment seg in freqDist.ObservedSamples)
				{
					if (freqDist[seg] == 1)
						n1++;
					else if (freqDist[seg] == 2)
						n2++;
					else if (freqDist[seg] > 2)
					{
						if (freqDist[seg] == 3)
							n3++;
						else if (freqDist[seg] == 4)
							n4++;
						nGreater++;
					}
				}
				ns.UpdateValue(cond.Item2 ?? string.Empty, () => Tuple.Create(0, 0, 0, 0), n => Tuple.Create(n.Item1 + n1, n.Item2 + n2, n.Item3 + n3, n.Item4 + n4));
				_bigNs[cond] = Tuple.Create(n1, n2, nGreater);
			}

			_discounts.Clear();
			foreach (KeyValuePair<string, Tuple<int, int, int, int>> kvp in ns)
			{
				double d1 = 0, d2 = 0, d3 = 0;
				double y = 0;
				if (kvp.Value.Item1 > 0)
				{
					y = (double) kvp.Value.Item1 / (kvp.Value.Item1 + (2 * kvp.Value.Item2));
					d1 = 1 - (2 * y * ((double) kvp.Value.Item2 / kvp.Value.Item1));
				}
				if (kvp.Value.Item2 > 0)
					d2 = 2 - (3 * y * ((double) kvp.Value.Item3 / kvp.Value.Item2));
				if (kvp.Value.Item3 > 0)
					d3 = 3 - (4 * y * ((double) kvp.Value.Item4 / kvp.Value.Item3));
				_discounts[kvp.Key] = Tuple.Create(d1, d2, d3);
			}

			if (ngramSize > 1)
				_lowerOrderModel = new NgramModel(segmentPool, ngramSize - 1, variety, dir, new ModifiedKneserNeySmoother());
		}

		public double GetProbability(Segment seg, Ngram context, string category)
		{
			FrequencyDistribution<Segment> freqDist = _cfd[Tuple.Create(context, category)];
			if (context.Count == 0)
				return (double) freqDist[seg] / freqDist.SampleOutcomeCount;
			
			if (freqDist.SampleOutcomeCount > 0)
			{
				int count = freqDist[seg];
				Tuple<double, double, double> discount = _discounts[category ?? string.Empty];
				Tuple<int, int, int> bigN = _bigNs[Tuple.Create(context, category)];
				double gamma = ((discount.Item1 * bigN.Item1) + (discount.Item2 * bigN.Item2) + (discount.Item3 * bigN.Item3)) / freqDist.SampleOutcomeCount;
				double d = 0;
				if (count == 1)
					d = discount.Item1;
				else if (count == 2)
					d = discount.Item2;
				else if (count > 2)
					d = discount.Item3;

				double prob = (count - d) / freqDist.SampleOutcomeCount;
				return prob + (gamma * _lowerOrderModel.GetProbability(seg, context.SkipFirst(_dir), category));
			}

			return 0;
		}

		public NgramModel LowerOrderModel
		{
			get { return _lowerOrderModel; }
		}
	}
}
