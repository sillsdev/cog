using QuickGraph;

namespace SIL.Cog.ViewModels
{
	public interface ILengthEdge<TVertex> : IEdge<TVertex>
	{
		double Length { get; }
	}
}
