using QuickGraph;

namespace SIL.Cog.Applications.GraphAlgorithms
{
	public interface IWeightedEdge<TVertex> : IEdge<TVertex>
	{
		double Weight { get; }
	}
}
