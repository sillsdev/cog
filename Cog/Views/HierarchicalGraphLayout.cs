using GraphSharp;
using GraphSharp.Controls;
using SIL.Cog.ViewModels;

namespace SIL.Cog.Views
{
	public class HierarchicalGraphLayout : GraphLayout<HierarchicalGraphVertex,
		TypedEdge<HierarchicalGraphVertex>, IHierarchicalBidirectionalGraph<HierarchicalGraphVertex, TypedEdge<HierarchicalGraphVertex>>> 
	{
	}
}
