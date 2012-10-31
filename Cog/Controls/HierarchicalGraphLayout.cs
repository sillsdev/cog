using GraphSharp;
using SIL.Cog.ViewModels;

namespace SIL.Cog.Controls
{
	public class HierarchicalGraphLayout : CogGraphLayout<HierarchicalGraphVertex,
		HierarchicalGraphEdge, IHierarchicalBidirectionalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge>> 
	{
		public HierarchicalGraphLayout()
		{
			HighlightAlgorithmFactory = new HierarchicalGraphHighlightAlgorithmFactory();
		}
	}
}
