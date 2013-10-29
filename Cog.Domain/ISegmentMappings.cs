using SIL.Machine;
using SIL.Machine.NgramModeling;

namespace SIL.Cog.Domain
{
	public interface ISegmentMappings
	{
		bool IsMapped(ShapeNode leftNode1, Ngram<Segment> target1, ShapeNode rightNode1, ShapeNode leftNode2, Ngram<Segment> target2, ShapeNode rightNode2);
	}
}
