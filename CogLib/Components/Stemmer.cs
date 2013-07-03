using System.Collections.Generic;
using System.Linq;
using SIL.Collections;
using SIL.Machine;
using SIL.Machine.FeatureModel;
using SIL.Machine.Matching;
using SIL.Machine.Rules;

namespace SIL.Cog.Components
{
	public class Stemmer : ProcessorBase<Variety>
	{
		private readonly SpanFactory<ShapeNode> _spanFactory;

		public Stemmer(SpanFactory<ShapeNode> spanFactory, CogProject project)
			: base(project)
		{
			_spanFactory = spanFactory;
		}

		public override void Process(Variety variety)
		{
			StemWords(Direction.LeftToRight, variety.Words, variety.Affixes.Where(a => a.Type == AffixType.Prefix));
			StemWords(Direction.RightToLeft, variety.Words, variety.Affixes.Where(a => a.Type == AffixType.Suffix));
		}

		private void StemWords(Direction dir, IEnumerable<Word> words, IEnumerable<Affix> affixes)
		{
			var ruleSpec = new BatchPatternRuleSpec<Word, ShapeNode>();
			foreach (Affix affix in affixes)
			{
				var pattern = new Pattern<Word, ShapeNode> {Acceptable = CheckStemWholeWord};
				if (dir == Direction.LeftToRight)
					pattern.Children.Add(new Constraint<Word, ShapeNode>(FeatureStruct.New().Symbol(CogFeatureSystem.AnchorType).Value));
				foreach (ShapeNode node in affix.Shape)
				{
					pattern.Children.Add(new Quantifier<Word, ShapeNode>(0, 1, new Constraint<Word, ShapeNode>(FeatureStruct.New().Symbol(CogFeatureSystem.BoundaryType).Value)));
					pattern.Children.Add(new Constraint<Word, ShapeNode>(node.Annotation.FeatureStruct.DeepClone()));
					pattern.Children.Add(new Quantifier<Word, ShapeNode>(0, 1, new Constraint<Word, ShapeNode>(FeatureStruct.New().Symbol(CogFeatureSystem.ToneLetterType).Value)));
				}
				if (dir == Direction.RightToLeft)
					pattern.Children.Add(new Constraint<Word, ShapeNode>(FeatureStruct.New().Symbol(CogFeatureSystem.AnchorType).Value));
				string category = affix.Category;
				ruleSpec.RuleSpecs.Add(new DefaultPatternRuleSpec<Word, ShapeNode>(pattern, MarkStem, word => category == null || word.Sense.Category == category));
			}

			var matcherSettings = new MatcherSettings<ShapeNode>
				{
					Direction = dir,
					Filter = ann => ann.Type().IsOneOf(CogFeatureSystem.ConsonantType, CogFeatureSystem.VowelType, CogFeatureSystem.AnchorType,
						CogFeatureSystem.ToneLetterType, CogFeatureSystem.BoundaryType)
				};
			var rule = new PatternRule<Word, ShapeNode>(_spanFactory, ruleSpec, matcherSettings);

			foreach (Word word in words.Where(w => w.Shape.Count > 0))
				rule.Apply(word);
		}

		private bool CheckStemWholeWord(Match<Word, ShapeNode> match)
		{
			return !match.Span.Contains(match.Input.Stem.Span);
		}

		private ShapeNode MarkStem(PatternRule<Word, ShapeNode> rule, Match<Word, ShapeNode> match, out Word output)
		{
			output = match.Input;
			Annotation<ShapeNode> stemAnn = output.Stem;
			int index = 0;
			foreach (ShapeNode node in output.Shape)
			{
				int len = node.OriginalStrRep().Length;
				bool finished = false;
				switch (rule.Matcher.Direction)
				{
					case Direction.LeftToRight:
						if (node == match.Span.End.Next)
							output.StemIndex = index;
						if (node == stemAnn.Span.End)
						{
							output.StemLength = index + len - output.StemIndex;
							finished = true;
						}
						break;

					case Direction.RightToLeft:
						if (node == stemAnn.Span.Start)
							output.StemIndex = index;
						if (node == match.Span.Start.Prev)
						{
							output.StemLength = index + len - output.StemIndex;
							finished = true;
						}
						break;
				}

				if (finished)
					break;
				index += len;
			}

			Project.Segmenter.Segment(output);
			return null;
		}
	}
}
