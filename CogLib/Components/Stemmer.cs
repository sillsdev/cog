using System.Collections.Generic;
using System.Diagnostics;
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
			foreach (Word word in variety.Words.Where(w => w.Shape.Count > 0 && (w.Prefix != null || w.Suffix != null)))
			{
				Shape shape;
				Project.Segmenter.ToShape(null, word.StrRep, null, out shape);
				word.Shape = shape;
			}

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

			var matcherSettings = new MatcherSettings<ShapeNode> {Direction = dir, Filter = ann => ann.Type().IsOneOf(CogFeatureSystem.ConsonantType, CogFeatureSystem.VowelType, CogFeatureSystem.AnchorType)};
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
			Annotation<ShapeNode> stemAnn = match.Input.Stem;

			var newShape = new Shape(_spanFactory, begin => new ShapeNode(_spanFactory, FeatureStruct.New().Symbol(CogFeatureSystem.AnchorType).Feature(CogFeatureSystem.StrRep).EqualTo("#").Value));

			ShapeNode startNode = null;
			ShapeNode endNode = null;
			foreach (ShapeNode node in match.Input.Shape)
			{
				ShapeNode newNode = node.DeepClone();
				newShape.Add(newNode);
				switch (rule.Matcher.Direction)
				{
					case Direction.LeftToRight:
						if (node == match.Span.End.Next)
							startNode = newNode;
						if (node == stemAnn.Span.End)
							endNode = newNode;
						break;

					case Direction.RightToLeft:
						if (node == stemAnn.Span.Start)
							startNode = newNode;
						if (node == match.Span.Start.Prev)
							endNode = newNode;
						break;
				}
			}

			Debug.Assert(startNode != null && endNode != null);
			if (startNode != newShape.First)
				newShape.Annotations.Add(newShape.First, startNode.Prev, FeatureStruct.New().Symbol(CogFeatureSystem.PrefixType).Value);
			newShape.Annotations.Add(startNode, endNode, FeatureStruct.New().Symbol(CogFeatureSystem.StemType).Value);
			if (endNode != newShape.Last)
				newShape.Annotations.Add(endNode.Next, newShape.Last, FeatureStruct.New().Symbol(CogFeatureSystem.SuffixType).Value);
			newShape.Freeze();
			output = match.Input;
			output.Shape = newShape;
			return null;
		}
	}
}
