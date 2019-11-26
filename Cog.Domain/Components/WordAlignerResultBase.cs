using System.Collections.Generic;
using System.Linq;
using SIL.Extensions;
using SIL.Machine.Annotations;
using SIL.Machine.SequenceAlignment;
using SIL.ObjectModel;

namespace SIL.Cog.Domain.Components
{
	internal abstract class WordAlignerResultBase : IWordAlignerResult
	{
		protected WordAlignerResultBase(IWordAligner wordAligner)
		{
			WordAligner = wordAligner;
		}

		protected static IEnumerable<ShapeNode> GetNodes(Word word, out int startIndex, out int count)
		{
			startIndex = 0;
			count = 0;
			var nodes = new List<ShapeNode>();
			Annotation<ShapeNode> stemAnn1 = word.Stem;
			foreach (ShapeNode node in word.Shape.Where(NodeFilter))
			{
				if (node.CompareTo(stemAnn1.Range.Start) < 0)
					startIndex++;
				else if (stemAnn1.Range.Contains(node))
					count++;
				nodes.Add(node);
			}
			return nodes;
		}

		protected static bool NodeFilter(ShapeNode n)
		{
			return n.Type().IsOneOf(CogFeatureSystem.ConsonantType, CogFeatureSystem.VowelType,
				CogFeatureSystem.AnchorType);
		}

		public IWordAligner WordAligner { get; }

		public abstract ReadOnlyList<Word> Words { get; }

		public abstract IEnumerable<Alignment<Word, ShapeNode>> GetAlignments();
		public abstract int BestRawScore { get; }
	}
}
