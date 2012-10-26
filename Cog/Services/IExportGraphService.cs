using GraphSharp;
using QuickGraph;
using SIL.Cog.ViewModels;

namespace SIL.Cog.Services
{
	public interface IExportGraphService
	{
		void ExportCurrentHierarchicalGraph(IHierarchicalBidirectionalGraph<HierarchicalGraphVertex, TypedEdge<HierarchicalGraphVertex>> graph, HierarchicalGraphType type, string path);
		void ExportCurrentNetworkGraph(IBidirectionalGraph<NetworkGraphVertex, NetworkGraphEdge> graph, string path);
	}
}
