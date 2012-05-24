using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SIL.Collections;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog
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

		public void Process(Variety variety)
		{
			IdentifyAffixes(variety, AffixType.Prefix);
			IdentifyAffixes(variety, AffixType.Suffix);
		}

		private void IdentifyAffixes(Variety variety, AffixType type)
		{
			Direction dir = Direction.LeftToRight;
			switch (type)
			{
				case AffixType.Prefix:
					dir = Direction.LeftToRight;
					break;

				case AffixType.Suffix:
					dir = Direction.RightToLeft;
					break;
			}

			var totalAffixFreqs = new Dictionary<int, FrequencyInfo>();
			var totalNonaffixFreqs = new Dictionary<int, FrequencyInfo>();

			var totalAffixes = new Dictionary<int, FrequencyInfo>();
			var totalNonaffixes = new Dictionary<int, FrequencyInfo>();

			var affixes = new Dictionary<string, AffixInfo>();
			var nonaffixes = new Dictionary<string, FrequencyInfo>();

			foreach (Word word in variety.Words)
			{
				if (word.Shape.Count == 1)
					continue;

				var sb = new StringBuilder();
				foreach (ShapeNode node in word.Shape.GetNodes(word.Shape.GetFirst(dir), word.Shape.GetLast(dir).GetPrev(dir), dir).Take(_maxAffixLength))
				{
					sb.Insert(dir == Direction.LeftToRight ? sb.Length : 0, (string) node.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep));
					string affixStr = sb.ToString();
					bool newAffix = !affixes.ContainsKey(affixStr);
					AffixInfo affixInfo = affixes.GetValue(affixStr, () =>
																		{
																			var shape = new Shape(_spanFactory,
																				begin => new ShapeNode(_spanFactory, FeatureStruct.New().Symbol(CogFeatureSystem.AnchorType).Value));
																			var span = _spanFactory.Create(word.Shape.GetFirst(dir), node, dir);
																			word.Shape.CopyTo(span, shape);
																			shape.Freeze();
																			return new AffixInfo(shape);
																		});
					affixInfo.Increment(word.Sense.Category);

					totalAffixFreqs.GetValue(affixInfo.Shape.Count, () => new FrequencyInfo()).Increment(word.Sense.Category);
					if (newAffix)
						totalAffixes.GetValue(affixInfo.Shape.Count, () => new FrequencyInfo()).Increment(word.Sense.Category);
				}

				foreach (ShapeNode node1 in word.Shape.GetFirst(dir).GetNext(dir).GetNodes(dir))
				{
					int count = 0;
					sb = new StringBuilder();
					foreach (ShapeNode node2 in node1.GetNodes(dir).Take(_maxAffixLength))
					{
						sb.Insert(dir == Direction.LeftToRight ? sb.Length : 0, node2.StrRep());
						count++;
						string nonaffixStr = sb.ToString();
						if (!nonaffixes.ContainsKey(nonaffixStr))
							totalNonaffixes.GetValue(count, () => new FrequencyInfo()).Increment(word.Sense.Category);

						nonaffixes.GetValue(nonaffixStr, () => new FrequencyInfo()).Increment(word.Sense.Category);
						totalNonaffixFreqs.GetValue(count, () => new FrequencyInfo()).Increment(word.Sense.Category);
					}
				}
			}

			foreach (AffixInfo affix in affixes.Values)
			{
				string category = affix.Categories.MaxBy(c => affix.GetFrequency(c));
				int freq = affix.Frequency;
				if (((double) affix.GetFrequency(category) / affix.Frequency) > 0.75)
				{
					affix.MainCategory = category;
					freq = affix.GetFrequency(category);
				}

				string affixStr = affix.ToString();
				//var caffixes = (from c in variety.Segments
				//                let ca = dir == Direction.LeftToRight ? affixStr + c.StrRep : c.StrRep + affixStr
				//                where affixes.ContainsKey(ca)
				//                orderby c.StrRep
				//                select new { Affix = affixes[ca], Segment = c }).ToArray();
				//var caffix = caffixes.Length == 0 ? null : caffixes.MaxBy(a => affix.MainCategory == null ? a.Affix.Frequency : a.Affix.GetFrequency(affix.MainCategory));
				//affix.CurveDrop = (1.0 - ((double)(caffix == null ? 0 : affix.MainCategory == null ? caffix.Affix.Frequency : caffix.Affix.GetFrequency(affix.MainCategory)) / freq))
				//    / (1.0 - (caffix == null ? 0 : caffix.Segment.Probability));

				var observed = new List<int>();
				int caffixTotalFreq = 0;
				foreach (Segment seg in variety.Segments)
				{
					string caffixStr = dir == Direction.LeftToRight ? affixStr + seg.StrRep : seg.StrRep + affixStr;
					AffixInfo ai;
					if (affixes.TryGetValue(caffixStr, out ai))
					{
						int f = affix.MainCategory == null ? ai.Frequency : ai.GetFrequency(affix.MainCategory);
						caffixTotalFreq += f;
						observed.Add(f);
					}
					else
						observed.Add(0);
				}

				//affix.CurveDrop = CosineSimilarity(observed, variety.Segments.Select(s => s.Probability * caffixTotalFreq).ToList());
				affix.CurveDrop = CosineSimilarity(observed, Enumerable.Repeat((double) caffixTotalFreq / variety.Segments.Count, variety.Segments.Count).ToList());
				//var probs = variety.Segments.Select(s => s.Probability).ToList();
				//affix.CurveDrop = MultinomialTest(observed, probs);

				//double pw = (double) (freq + 1) / (totalAffixFreqs[affix.Shape.Count] + totalAffixes[affix.Shape.Count]);

				int nfreq = 0;
				FrequencyInfo nonaffixInfo;
				if (nonaffixes.TryGetValue(affixStr, out nonaffixInfo))
					nfreq = affix.MainCategory == null ? nonaffixInfo.Frequency : nonaffixInfo.GetFrequency(affix.MainCategory);

				//double npw = (double) (nfreq + 1) / (totalNonaffixFreqs[affix.Shape.Count] + totalNonaffixes[affix.Shape.Count]);

				//affix.RandomAdjustment = pw / npw;

				affix.RandomAdjustment = (double) (freq + 1) / (freq + nfreq + 2);

				FrequencyInfo totalAffixFreqInfo = totalAffixFreqs[affix.Shape.Count];
				double adjustedFreq = (double) freq / (affix.MainCategory == null ? totalAffixFreqInfo.Frequency : totalAffixFreqInfo.GetFrequency(affix.MainCategory));

				const double alpha = 0.33;
				const double beta = 0.33;
				affix.Score = (alpha * affix.CurveDrop) + (beta * affix.RandomAdjustment) + ((1.0 - (alpha + beta)) * adjustedFreq);
				//affix.Score = affix.CurveDrop * affix.RandomAdjustment * (affix.Shape.Count * (5 * Math.Log(freq)));
				//affix.Score = affix.CurveDrop * affix.RandomAdjustment * ((affix.Shape.Count * freq) / 2.0);
				//affix.Score = affix.CurveDrop * affix.RandomAdjustment * freq;
			}

			foreach (AffixInfo affixInfo in affixes.Values.Where(p => p.Score >= _threshold && (!_categoryRequired || p.MainCategory != null)).OrderByDescending(p => p.Score))
			{
				string affixStr = affixInfo.ToString();
				if (variety.Affixes.All(a => (a.Category != null && affixInfo.MainCategory != null && a.Category != affixInfo.MainCategory)
					|| (dir == Direction.LeftToRight ? !affixStr.StartsWith(a.StrRep) : !affixStr.EndsWith(a.StrRep))))
				{
					variety.Affixes.Add(new Affix(type, affixInfo.Shape, affixInfo.MainCategory) {Score = affixInfo.Score});
				}
			}
		}

		private static double CosineSimilarity(IList<int> observed, IList<double> expected)
		{
			double dot = observed.Zip(expected).Sum(tuple => tuple.Item1 * tuple.Item2);
			double obsMag = Math.Sqrt(observed.Sum(o => o * o));
			double expMag = Math.Sqrt(expected.Sum(o => o * o));
			return dot / (obsMag * expMag);
		}

		private class FrequencyInfo
		{
			private int _frequency;
			private readonly Dictionary<string, int> _categoryFrequencies;

			public FrequencyInfo()
			{
				_categoryFrequencies = new Dictionary<string, int>();
			}

			public int Frequency
			{
				get { return _frequency; }
			}

			public IEnumerable<string> Categories
			{
				get { return _categoryFrequencies.Keys; }
			}

			public void Increment(string category)
			{
				_frequency++;
				_categoryFrequencies.UpdateValue(category, () => 0, freq => freq + 1);
			}

			public int GetFrequency(string category)
			{
				int freq;
				if (_categoryFrequencies.TryGetValue(category, out freq))
					return freq;
				return 0;
			}
		}

		private class AffixInfo : FrequencyInfo
		{
			private readonly Shape _shape;

			public AffixInfo(Shape shape)
			{
				_shape = shape;
			}

			public Shape Shape
			{
				get { return _shape; }
			}

			public double CurveDrop { get; set; }
			public double RandomAdjustment { get; set; }
			public double Score { get; set; }

			public string MainCategory { get; set; }

			public override string ToString()
			{
				return string.Concat(_shape.Select(node => (string) node.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep)));
			}
		}
	}
}
