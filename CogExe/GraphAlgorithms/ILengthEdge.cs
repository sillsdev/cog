using QuickGraph;

namespace SIL.Cog.GraphAlgorithms
{
	public interface ILengthEdge<TVertex> : IEdge<TVertex>
	{
		double Length { get; }
	}
}
