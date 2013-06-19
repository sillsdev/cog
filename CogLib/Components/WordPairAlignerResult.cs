using System.Collections.Generic;
using System.Linq;
using SIL.Cog.SequenceAlignment;
using SIL.Collections;
using SIL.Machine;

namespace SIL.Cog.Components
{
	public class WordPairAlignerResult : IWordPairAlignerResult
	{
		private readonly PairwiseAlignmentAlgorithm<ShapeNode> _algorithm;
		private bool _computed;

		internal WordPairAlignerResult(WordPairAlignerSettings settings, IPairwiseAlignmentScorer<ShapeNode> scorer, Word word1, Word word2)
		{
			int startIndex1, count1;
			IEnumerable<ShapeNode> sequence1 = GetNodes(word1, out startIndex1, out count1);
			int startIndex2, count2;
			IEnumerable<ShapeNode> sequence2 = GetNodes(word2, out startIndex2, out count2);
			_algorithm = new PairwiseAlignmentAlgorithm<ShapeNode>(scorer, sequence1, startIndex1, count1, sequence2, startIndex2, count2)
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

		public IEnumerable<Alignment<ShapeNode>> GetAlignments()
		{
			if (!_computed)
			{
				_algorithm.Compute();
				_computed = true;
			}
			return _algorithm.GetAlignments();
		}

		public IEnumerable<Alignment<ShapeNode>> GetAlignments(double scoreMargin)
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
