using System.Collections.Generic;
using System.Linq;
using SIL.Collections;
using SIL.Machine;
using SIL.Machine.FeatureModel;
using SIL.Machine.Matching;
using SIL.Machine.Rules;

namespace SIL.Cog
{
	public class Stemmer : IProcessor<Variety>
	{
		private readonly SpanFactory<ShapeNode> _spanFactory; 

		public Stemmer(SpanFactory<ShapeNode> spanFactory)
		{
			_spanFactory = spanFactory;
		}

		public void Process(Variety variety)
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
					pattern.Children.Add(new Constraint<Word, ShapeNode>(node.Annotation.FeatureStruct.DeepClone()));
				if (dir == Direction.RightToLeft)
					pattern.Children.Add(new Constraint<Word, ShapeNode>(FeatureStruct.New().Symbol(CogFeatureSystem.AnchorType).Value));
				string category = affix.Category;
				ruleSpec.RuleSpecs.Add(new DefaultPatternRuleSpec<Word, ShapeNode>(pattern, MarkStem, word => category == null || word.Sense.Category == category));
			}

			var matcherSettings = new MatcherSettings<ShapeNode> {FastCompile = true, Direction = dir};
			var rule = new PatternRule<Word, ShapeNode>(_spanFactory, ruleSpec, matcherSettings);

			foreach (Word word in words)
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
			switch (rule.Matcher.Direction)
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
	}
}
