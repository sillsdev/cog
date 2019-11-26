using System;
using System.Collections.Generic;
using System.Linq;
using SIL.Machine.Annotations;
using SIL.Machine.SequenceAlignment;
using SIL.ObjectModel;

namespace SIL.Cog.Domain.Components
{
	internal class SingleWordAlignmentResult : WordAlignerResultBase
	{
		private readonly Alignment<Word, ShapeNode> _alignment;

		public SingleWordAlignmentResult(IWordAligner wordAligner, Word word)
			: base(wordAligner)
		{
			Words = new ReadOnlyList<Word>(new[] { word });

			Annotation<ShapeNode> prefixAnn = word.Prefix;
			var prefix = new AlignmentCell<ShapeNode>(prefixAnn != null
				? word.Shape.GetNodes(prefixAnn.Range).Where(NodeFilter)
				: Enumerable.Empty<ShapeNode>());
			IEnumerable<AlignmentCell<ShapeNode>> columns = word.Shape.GetNodes(word.Stem.Range).Where(NodeFilter)
				.Select(n => new AlignmentCell<ShapeNode>(n));
			Annotation<ShapeNode> suffixAnn = word.Suffix;
			var suffix = new AlignmentCell<ShapeNode>(suffixAnn != null
				? word.Shape.GetNodes(suffixAnn.Range).Where(NodeFilter)
				: Enumerable.Empty<ShapeNode>());
			_alignment = new Alignment<Word, ShapeNode>(0, 0, Tuple.Create(word, prefix, columns, suffix));
		}

		public override ReadOnlyList<Word> Words { get; }

		public override int BestRawScore => 0;

		public override IEnumerable<Alignment<Word, ShapeNode>> GetAlignments()
		{
			yield return _alignment;
		}
	}
}
