using System.Collections.Generic;
using System.Linq;
using SIL.Collections;
using SIL.Machine.Annotations;
using SIL.Machine.SequenceAlignment;

namespace SIL.Cog.Domain.Components
{
	internal abstract class WordAlignerResultBase : IWordAlignerResult
	{
		private readonly IWordAligner _wordAligner;

		protected WordAlignerResultBase(IWordAligner wordAligner)
		{
			_wordAligner = wordAligner;
		}

		protected static IEnumerable<ShapeNode> GetNodes(Word word, out int startIndex, out int count)
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

		public IWordAligner WordAligner
		{
			get { return _wordAligner; }
		}

		public abstract IReadOnlyList<Word> Words { get; }

		public abstract IEnumerable<Alignment<Word, ShapeNode>> GetAlignments();
		public abstract int BestRawScore { get; }
	}
}
