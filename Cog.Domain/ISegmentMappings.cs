namespace SIL.Cog.Domain
{
	public interface ISegmentMappings
	{
		bool IsMapped(Segment seg1, Segment seg2);
	}
}
