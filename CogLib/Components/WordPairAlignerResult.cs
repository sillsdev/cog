using System.Collections.Generic;
using System.Linq;
using SIL.Cog.SequenceAlignment;
using SIL.Collections;
using SIL.Machine;

namespace SIL.Cog.Components
{
	public class WordPairAlignerResult : IWordPairAlignerResult
	{
		private readonly PairwiseAlignmentAlgorithm<Word, ShapeNode> _algorithm;
		private bool _computed;

		internal WordPairAlignerResult(IPairwiseAlignmentScorer<Word, ShapeNode> scorer, WordPairAlignerSettings settings, Word word1, Word word2)
		{
			_algorithm = new PairwiseAlignmentAlgorithm<Word, ShapeNode>(scorer, word1, word2, GetNodes)
				{
					ExpansionCompressionEnabled = settings.ExpansionCompressionEnabled,
					Mode = settings.Mode
				};
		}

		private static IEnumerable<ShapeNode> GetNodes(Word word, out int startIndex, out int count)
		{
			startIndex = 0;
			count = 0;
			var nodes = new List<ShapeNode>();
			Annotation<ShapeNode> stemAnn1 = word.Stem;
			foreach (ShapeNode node in word.Shape.Where(n => n.Type().IsOneOf(CogFeatureSystem.ConsonantType, CogFeatureSystem.VowelType, CogFeatureSystem.AnchorType)))
			{
				if (node.CompareTo(stemAnn1.Span.Start) < 0)
					startIndex++;
				else if (stemAnn1.Span.Contains(node))
					count++;
				nodes.Add(node);
			}
			return nodes;
		}

		public int BestRawScore
		{
			get
			{
				if (!_computed)
				{
					_algorithm.Compute();
					_computed = true;
				}
				return _algorithm.BestRawScore;
			}
		}

		public IEnumerable<Alignment<Word, ShapeNode>> GetAlignments()
		{
			if (!_computed)
			{
				_algorithm.Compute();
				_computed = true;
			}
			return _algorithm.GetAlignments();
		}

		public IEnumerable<Alignment<Word, ShapeNode>> GetAlignments(double scoreMargin)
		{
			if (!_computed)
			{
				_algorithm.Compute();
				_computed = true;
			}
			return _algorithm.GetAlignments(scoreMargin);
		}
	}
}
