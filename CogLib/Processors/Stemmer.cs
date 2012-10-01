using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SIL.Collections;
using SIL.Machine;
using SIL.Machine.FeatureModel;
using SIL.Machine.Matching;
using SIL.Machine.Rules;

namespace SIL.Cog.Processors
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

		private void StemWords(Direction dir, WordCollection words, IEnumerable<Affix> affixes)
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


			foreach (Word word in words.ToArray())
			{
				words.Remove(word);
				Word newWord = rule.Apply(word).SingleOrDefault();
				if (newWord != null)
				{
					words.Add(newWord);
				}
				else
				{
					var newShape = new Shape(_spanFactory, begin => new ShapeNode(_spanFactory, FeatureStruct.New().Symbol(CogFeatureSystem.AnchorType).Value));
					newShape.AddRange(word.Shape.Select(n => n.DeepClone()));
					newShape.Freeze();
					words.Add(new Word(word.StrRep, newShape, word.Sense));
				}
			}
		}

		private bool CheckStemWholeWord(Match<Word, ShapeNode> match)
		{
			Annotation<ShapeNode> stemAnn = match.Input.Annotations.SingleOrDefault(ann => ann.Type() == CogFeatureSystem.StemType);
			Span<ShapeNode> span = stemAnn != null ? stemAnn.Span : _spanFactory.Create(match.Input.Shape.First, match.Input.Shape.Last);
			return !match.Span.Contains(span);
		}

		private ShapeNode MarkStem(PatternRule<Word, ShapeNode> rule, Match<Word, ShapeNode> match, out Word output)
		{
			Annotation<ShapeNode> stemAnn = match.Input.Annotations.SingleOrDefault(ann => ann.Type() == CogFeatureSystem.StemType);

			var newShape = new Shape(_spanFactory, begin => new ShapeNode(_spanFactory, FeatureStruct.New().Symbol(CogFeatureSystem.AnchorType).Value));

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
						if ((stemAnn != null && node == stemAnn.Span.End) || (stemAnn == null && node == match.Input.Shape.Last))
							endNode = newNode;
						break;

					case Direction.RightToLeft:
						if ((stemAnn != null && node == stemAnn.Span.Start) || (stemAnn == null && node == match.Input.Shape.First))
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
			output = new Word(match.Input.StrRep, newShape, match.Input.Sense);
			return null;
		}
	}
}
