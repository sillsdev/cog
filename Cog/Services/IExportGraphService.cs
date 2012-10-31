using GraphSharp;
using QuickGraph;
using SIL.Cog.ViewModels;

namespace SIL.Cog.Services
{
	public interface IExportGraphService
	{
		void ExportCurrentHierarchicalGraph(HierarchicalGraphType type, string path);
		void ExportHierarchicalGraph(IHierarchicalBidirectionalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge> graph, HierarchicalGraphType graphType, string path);
		void ExportCurrentNetworkGraph(string path);
		void ExportNetworkGraph(IBidirectionalGraph<NetworkGraphVertex, NetworkGraphEdge> graph, string path);
	}
}
