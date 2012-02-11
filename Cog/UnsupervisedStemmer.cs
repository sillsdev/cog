using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SIL.Machine;
using SIL.Machine.FeatureModel;
using SIL.Machine.Matching;
using SIL.Machine.Transduction;

namespace SIL.Cog
{
	public class UnsupervisedStemmer : IProcessor<Variety>
	{
		private readonly SpanFactory<ShapeNode> _spanFactory;
		private readonly double _threshold;
		private readonly int _maxAffixLength;

		public UnsupervisedStemmer(SpanFactory<ShapeNode> spanFactory, double threshold, int maxAffixLength)
		{
			_spanFactory = spanFactory;
			_threshold = threshold;
			_maxAffixLength = maxAffixLength;
		}

		public void Process(Variety variety)
		{
			Stem(variety, AffixType.Prefix);
			Stem(variety, AffixType.Suffix);
		}

		private void Stem(Variety variety, AffixType type)
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

			var totalAffixFreqs = new Dictionary<int, int>();
			var totalNonaffixFreqs = new Dictionary<int, int>();

			var affixes = new Dictionary<string, AffixInfo>();
			var nonaffixes = new Dictionary<string, int>();

			foreach (Word word in variety.Words)
			{
				if (word.Shape.Count == 1)
					continue;

				var sb = new StringBuilder();
				foreach (ShapeNode node in word.Shape.GetNodes(word.Shape.GetFirst(dir), word.Shape.GetLast(dir).GetPrev(dir), dir).Take(_maxAffixLength))
				{
					sb.Insert(dir == Direction.LeftToRight ? sb.Length : 0, (string) node.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep));
					AffixInfo affixInfo = affixes.GetValue(sb.ToString(), () =>
																			{
																				var shape = new Shape(_spanFactory,
																					new ShapeNode(_spanFactory, FeatureStruct.New().Symbol(CogFeatureSystem.AnchorType).Value),
																					new ShapeNode(_spanFactory, FeatureStruct.New().Symbol(CogFeatureSystem.AnchorType).Value));
																				var span = _spanFactory.Create(word.Shape.GetFirst(dir), node, dir);
																				word.Shape.CopyTo(span, shape);
																				return new AffixInfo(shape);
																			});
					affixInfo.Increment(word.Category);

					totalAffixFreqs.UpdateValue(affixInfo.Shape.Count, () => 0, freq => freq + 1);
				}

				foreach (ShapeNode node1 in word.Shape.GetFirst(dir).GetNext(dir).GetNodes(dir))
				{
					int count = 0;
					sb = new StringBuilder();
					foreach (ShapeNode node2 in node1.GetNodes(dir).Take(_maxAffixLength))
					{
						sb.Insert(dir == Direction.LeftToRight ? sb.Length : 0, node2.StrRep());
						count++;
						nonaffixes.UpdateValue(sb.ToString(), () => 0, freq => freq + 1);
						totalNonaffixFreqs.UpdateValue(count, () => 0, freq => freq + 1);
					}
				}
			}

			foreach (AffixInfo affix in affixes.Values)
			{
				string affixStr = affix.ToString();
				var caffixes = (from c in variety.Phonemes
								let ca = dir == Direction.LeftToRight ? affixStr + c.StrRep : c.StrRep + affixStr
								where affixes.ContainsKey(ca)
								select new { Affix = affixes[ca], Phoneme = c }).ToArray();
				var caffix = caffixes.Length == 0 ? null : caffixes.MaxBy(a => a.Affix.Frequency);
				affix.CurveDrop = (1.0 - ((double)(caffix == null ? 0 : caffix.Affix.Frequency) / affix.Frequency)) / (1.0 - (caffix == null ? 0 : caffix.Phoneme.Probability));

				double pw = (double) affix.Frequency / totalAffixFreqs[affix.Shape.Count];

				int nfreq = nonaffixes.GetValue(affixStr, () => 0);

				double npw = nfreq == 0 ? (1.0 / affix.Shape.Count) / totalNonaffixFreqs[affix.Shape.Count] : (double) nfreq / totalNonaffixFreqs[affix.Shape.Count];

				affix.RandomAdjustment = pw / npw;

				affix.Score = affix.CurveDrop * affix.RandomAdjustment * (affix.Shape.Count * Math.Log(affix.Frequency));
				//affix.Score = affix.CurveDrop * affix.RandomAdjustment * affix.Frequency;

				foreach (string category in affix.Categories)
				{
					if (((double) affix.GetFrequency(category) / affix.Frequency) > 0.75)
					{
						affix.MainCategory = category;
						break;
					}
				}
			}

			var ruleSpec = new BatchPatternRuleSpec<Word, ShapeNode>();
			var matcherSettings = new MatcherSettings<ShapeNode> { Quasideterministic = true, Direction = dir };
			foreach (AffixInfo affixInfo in affixes.Values.Where(p => p.Score >= _threshold).OrderByDescending(p => p.Score))
			{
				string affixStr = affixInfo.ToString();
				if (variety.Affixes.All(a => (a.Category != null && affixInfo.MainCategory != null && a.Category != affixInfo.MainCategory)
					|| (dir == Direction.LeftToRight ? !affixStr.StartsWith(a.StrRep) : !affixStr.EndsWith(a.StrRep))))
				{
					var pattern = new Pattern<Word, ShapeNode> {Acceptable = CheckStemWholeWord};
					if (dir == Direction.LeftToRight)
						pattern.Children.Add(new Constraint<Word, ShapeNode>(FeatureStruct.New().Symbol(CogFeatureSystem.AnchorType).Value));
					foreach (ShapeNode node in affixInfo.Shape)
						pattern.Children.Add(new Constraint<Word, ShapeNode>(node.Annotation.FeatureStruct.Clone()));
					if (dir == Direction.RightToLeft)
						pattern.Children.Add(new Constraint<Word, ShapeNode>(FeatureStruct.New().Symbol(CogFeatureSystem.AnchorType).Value));
					string category = affixInfo.MainCategory;
					ruleSpec.AddRuleSpec(new DefaultPatternRuleSpec<Word, ShapeNode>(pattern, MarkStem, word => category == null || word.Category == category));
					variety.AddAffix(new Affix(affixStr, type, affixInfo.MainCategory) {Score = affixInfo.Score});
				}
			}

			var rule = new PatternRule<Word, ShapeNode>(_spanFactory, ruleSpec, matcherSettings);

			foreach (Word word in variety.Words)
				rule.Apply(word);
		}

		private bool CheckStemWholeWord(Match<Word, ShapeNode> match)
		{
			Annotation<ShapeNode> stemAnn = match.Input.Annotations.SingleOrDefault(ann => ann.Type() == CogFeatureSystem.StemType);
			Span<ShapeNode> span = stemAnn != null ? stemAnn.Span : _spanFactory.Create(match.Input.Shape.First, match.Input.Shape.Last);
			return !match.Span.Contains(span);
		}

		private ShapeNode MarkStem(PatternRule<Word, ShapeNode> rule, Match<Word, ShapeNode> match, out Word output)
		{
			output = match.Input;
			Annotation<ShapeNode> stemAnn = output.Annotations.SingleOrDefault(ann => ann.Type() == CogFeatureSystem.StemType);
			ShapeNode startNode = null;
			ShapeNode endNode = null;
			switch (rule.MatcherSettings.Direction)
			{
				case Direction.LeftToRight:
					startNode = match.Span.End.Next;
					endNode = stemAnn == null ? output.Shape.Last : stemAnn.Span.End;
					break;

				case Direction.RightToLeft:
					startNode = stemAnn == null ? output.Shape.First : stemAnn.Span.Start;
					endNode = match.Span.Start.Prev;
					break;
			}
			if (stemAnn != null)
				stemAnn.Remove();
			output.Annotations.Add(startNode, endNode, FeatureStruct.New().Symbol(CogFeatureSystem.StemType).Value);
			return null;
		}

		private class AffixInfo
		{
			private readonly Shape _shape;
			private int _frequency;
			private readonly Dictionary<string, int> _categoryFrequencies; 

			public AffixInfo(Shape shape)
			{
				_shape = shape;
				_categoryFrequencies = new Dictionary<string, int>();
			}

			public Shape Shape
			{
				get { return _shape; }
			}

			public int Frequency
			{
				get { return _frequency; }
			}

			public double CurveDrop { get; set; }
			public double RandomAdjustment { get; set; }
			public double Score { get; set; }

			public IEnumerable<string> Categories
			{
				get { return _categoryFrequencies.Keys; }
			}

			public string MainCategory { get; set; }

			public void Increment(string category)
			{
				_frequency++;
				_categoryFrequencies.UpdateValue(category, () => 0, freq => freq + 1);
			}

			public int GetFrequency(string category)
			{
				return _categoryFrequencies[category];
			}

			public override string ToString()
			{
				return string.Concat(_shape.Select(node => (string) node.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep)));
			}
		}
	}
}
