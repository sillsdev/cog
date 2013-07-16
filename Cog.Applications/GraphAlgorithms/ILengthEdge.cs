using QuickGraph;

namespace SIL.Cog.Applications.GraphAlgorithms
{
	public interface ILengthEdge<TVertex> : IEdge<TVertex>
	{
		double Length { get; }
	}
}
