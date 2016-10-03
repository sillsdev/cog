using System.Collections.Generic;
using System.Linq;
using SIL.Collections;
using SIL.Machine.Annotations;
using SIL.Machine.NgramModeling;

namespace SIL.Cog.Domain.Components
{
	public class UnionSegmentMappings : ISegmentMappings
	{
		private readonly ReadOnlyList<ISegmentMappings> _segmentMappingsComponents; 

		public UnionSegmentMappings(IEnumerable<ISegmentMappings> segmentMappingsComponents)
		{
			_segmentMappingsComponents = new ReadOnlyList<ISegmentMappings>(segmentMappingsComponents.ToList());
		}

		public ReadOnlyList<ISegmentMappings> SegmentMappingsComponents
		{
			get { return _segmentMappingsComponents; }
		}

		public bool IsMapped(ShapeNode leftNode1, Ngram<Segment> target1, ShapeNode rightNode1, ShapeNode leftNode2, Ngram<Segment> target2, ShapeNode rightNode2)
		{
			return _segmentMappingsComponents.Any(sm => sm.IsMapped(leftNode1, target1, rightNode1, leftNode2, target2, rightNode2));
		}
	}
}
