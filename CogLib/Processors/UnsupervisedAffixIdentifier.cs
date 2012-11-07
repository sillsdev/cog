using System;
using System.Collections.Generic;
using System.Linq;
using SIL.Cog.NgramModeling;
using SIL.Cog.Statistics;
using SIL.Collections;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Processors
{
	public class UnsupervisedAffixIdentifier : IProcessor<Variety>
	{
		private readonly SpanFactory<ShapeNode> _spanFactory; 
		private readonly double _threshold;
		private readonly int _maxAffixLength;
		private readonly bool _categoryRequired;

		public UnsupervisedAffixIdentifier(SpanFactory<ShapeNode> spanFactory, double threshold, int maxAffixLength, bool categoryRequired)
		{
			_spanFactory = spanFactory;
			_threshold = threshold;
			_maxAffixLength = maxAffixLength;
			_categoryRequired = categoryRequired;
		}

		public double Threshold
		{
			get { return _threshold; }
		}

		public int MaxAffixLength
		{
			get { return _maxAffixLength; }
		}

		public bool CategoryRequired
		{
			get { return _categoryRequired; }
		}

		public void Process(Variety variety)
		{
			if (variety.Affixes.Count == 0)
			{
				IdentifyAffixes(variety, AffixType.Prefix);
				IdentifyAffixes(variety, AffixType.Suffix);
			}
		}

		private Affix CreateAffix(Direction dir, Ngram ngram, string category, double score)
		{
			var shape = new Shape(_spanFactory,
				begin => new ShapeNode(_spanFactory, FeatureStruct.New().Symbol(CogFeatureSystem.AnchorType).Value));
			foreach (Segment seg in ngram)
			{
				if (seg.Type != CogFeatureSystem.AnchorType)
					shape.AddAfter(shape.GetLast(dir), seg.FeatureStruct, dir);
			}
			shape.Freeze();
			return new Affix(string.Concat(shape.Select(n => n.StrRep())), dir == Direction.LeftToRight ? AffixType.Prefix : AffixType.Suffix,
				shape, category == string.Empty ? null : category) {Score = score};
		}

		private static bool Filter(ShapeNode node)
		{
			return node.Annotation.Type().IsOneOf(CogFeatureSystem.ConsonantType, CogFeatureSystem.VowelType);
		}

		private void IdentifyAffixes(Variety variety, AffixType type)
		{
			var dir = Direction.LeftToRight;
			switch (type)
			{
				case AffixType.Prefix:
					dir = Direction.LeftToRight;
					break;

				case AffixType.Suffix:
					dir = Direction.RightToLeft;
					break;
			}

			IReadOnlyList<NgramModel> ngramModels = NgramModel.BuildAll(_maxAffixLength + 2, variety, dir);
			var affixFreqDist = new ConditionalFrequencyDistribution<Tuple<int, string>, Ngram>();
			var ngramFreqDist = new ConditionalFrequencyDistribution<Tuple<int, string>, Ngram>();

			var candidates = new HashSet<Ngram>();
			var categories = new HashSet<string>();
			foreach (Word word in variety.Words)
			{
				if (word.Shape.Count < 2)
					continue;

				if (!string.IsNullOrEmpty(word.Sense.Category))
					categories.Add(word.Sense.Category);

				var affix = new Ngram(Segment.Anchor);
				foreach (ShapeNode node in word.Shape.GetNodes(word.Shape.GetFirst(dir), word.Shape.GetLast(dir).GetPrev(dir), dir).Where(Filter).Take(_maxAffixLength))
				{
					if (node == word.Shape.GetLast(dir, Filter))
						break;

					Segment seg = variety.Segments[node];
					affix = new Ngram(affix.Concat(seg));
					candidates.Add(affix);
					affixFreqDist[Tuple.Create(affix.Count, string.Empty)].Increment(affix);
					ngramFreqDist[Tuple.Create(affix.Count, string.Empty)].Increment(affix);
					if (!string.IsNullOrEmpty(word.Sense.Category))
					{
						affixFreqDist[Tuple.Create(affix.Count, word.Sense.Category)].Increment(affix);
						ngramFreqDist[Tuple.Create(affix.Count, word.Sense.Category)].Increment(affix);
					}	
				}

				var segs = word.Shape.GetNodes(dir).Where(Filter).Select(n => variety.Segments[n]);
				var wordNgram = new Ngram(Segment.Null.ToEnumerable().Concat(segs));
				if (wordNgram.Count <= _maxAffixLength + 1)
				{
					ngramFreqDist[Tuple.Create(wordNgram.Count, string.Empty)].Increment(wordNgram);
					if (!string.IsNullOrEmpty(word.Sense.Category))
						ngramFreqDist[Tuple.Create(wordNgram.Count, word.Sense.Category)].Increment(wordNgram);
				}

				foreach (ShapeNode node1 in word.Shape.GetFirst(dir).GetNext(dir).GetNodes(dir).Where(Filter))
				{
					var nonaffix = new Ngram(Segment.Null);
					foreach (ShapeNode node2 in node1.GetNodes(dir).Where(Filter).Take(_maxAffixLength))
					{
						Segment seg = variety.Segments[node2];
						nonaffix = new Ngram(nonaffix.Concat(seg));
						ngramFreqDist[Tuple.Create(nonaffix.Count, string.Empty)].Increment(nonaffix);
						if (!string.IsNullOrEmpty(word.Sense.Category))
							ngramFreqDist[Tuple.Create(nonaffix.Count, word.Sense.Category)].Increment(nonaffix);
					}
				}
			}

			var affixProbDist = new ConditionalProbabilityDistribution<Tuple<int, string>, Ngram>(affixFreqDist, fd => new SimpleGoodTuringProbabilityDistribution<Ngram>(fd, fd.ObservedSamples.Count + 1));
			var nonaffixProbDist = new ConditionalProbabilityDistribution<Tuple<int, string>, Ngram>(ngramFreqDist, fd => new MaxLikelihoodProbabilityDistribution<Ngram>(fd));

			var affixes = new List<Affix>();
			foreach (Ngram candidate in candidates)
			{
				string category = string.Empty;
				if (categories.Count > 0)
				{
					category = categories.MaxBy(c => affixFreqDist[Tuple.Create(candidate.Count, c)][candidate]);
					if (((double) affixFreqDist[Tuple.Create(candidate.Count, category)][candidate] / affixFreqDist[Tuple.Create(candidate.Count, string.Empty)][candidate]) <= 0.75)
						category = string.Empty;
				}
				NgramModel higherOrderModel = ngramModels[candidate.Count];
				double curveDrop = CosineSimilarity(variety.Segments.Select(seg => higherOrderModel.GetProbability(seg, candidate, category)),
					variety.Segments.Select(seg => ngramModels[0].GetProbability(seg, new Ngram(), category)));

				double affixProb = nonaffixProbDist[Tuple.Create(candidate.Count, category)].GetProbability(candidate);
				double nonaffixProb = nonaffixProbDist[Tuple.Create(candidate.Count, category)].GetProbability(new Ngram(Segment.Null.ToEnumerable().Concat(candidate.Skip(1))));
				double diff = affixProb - nonaffixProb;
				diff = Math.Min(diff, 0.75);
				diff = Math.Max(diff, -0.25);
				double randomAdj = diff + 0.25;
				//randomAdj = Math.Min(50, prob / nonaffixProb) / 50;
				//int freq = curModel.GetFrequency(ngram, category);
				//int nfreq = variety.Segments.Sum(seg => curModel.GetFrequency(new Ngram(seg.ToEnumerable().Concat(ngram.Skip(1)))));
				//randomAdj = (double) freq / (freq + nfreq);

				double prob = affixProbDist[Tuple.Create(candidate.Count, category)].GetProbability(candidate);

				const double alpha = 0.33;
				const double beta = 0.33;
				double score = (alpha * curveDrop) + (beta * randomAdj) + ((1.0 - (alpha + beta)) * prob);

				if (score >= _threshold && (!_categoryRequired || !string.IsNullOrEmpty(category)))
					affixes.Add(CreateAffix(dir, candidate, category, score));
			}


			foreach (Affix affix in affixes.OrderByDescending(a => a.Score))
			{
				string affixStr = affix.StrRep;
				if (variety.Affixes.All(a => (a.Category != null && affix.Category != null && a.Category != affix.Category)
					|| (type == AffixType.Prefix ? !affixStr.StartsWith(a.StrRep) : !affixStr.EndsWith(a.StrRep))))
				{
					variety.Affixes.Add(affix);
				}
			}
		}

		private static double CosineSimilarity(IEnumerable<double> observed, IEnumerable<double> expected)
		{
			double dot = 0, obsTotal = 0, expTotal = 0;
			foreach (Tuple<double, double> t in observed.Zip(expected))
			{
				dot += t.Item1 * t.Item2;
				obsTotal += t.Item1 * t.Item1;
				expTotal += t.Item2 * t.Item2;
			}
			return dot / (Math.Sqrt(obsTotal) * Math.Sqrt(expTotal));
		}
	}
}
