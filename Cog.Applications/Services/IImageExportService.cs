using QuickGraph;
using SIL.Cog.Applications.ViewModels;

namespace SIL.Cog.Applications.Services
{
	public interface IImageExportService
	{
		bool ExportCurrentHierarchicalGraph(object ownerViewModel, HierarchicalGraphType type);
		bool ExportHierarchicalGraph(object ownerViewModel, IBidirectionalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge> graph, HierarchicalGraphType graphType);
		bool ExportCurrentNetworkGraph(object ownerViewModel);
		bool ExportNetworkGraph(object ownerViewModel, IBidirectionalGraph<NetworkGraphVertex, NetworkGraphEdge> graph, double scoreFilter);
		bool ExportCurrentMap(object ownerViewModel);
	}
}
