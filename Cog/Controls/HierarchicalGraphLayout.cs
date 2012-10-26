using GraphSharp;
using SIL.Cog.ViewModels;

namespace SIL.Cog.Controls
{
	public class HierarchicalGraphLayout : CogGraphLayout<HierarchicalGraphVertex,
		TypedEdge<HierarchicalGraphVertex>, IHierarchicalBidirectionalGraph<HierarchicalGraphVertex, TypedEdge<HierarchicalGraphVertex>>> 
	{
	}
}
