using GraphSharp;
using QuickGraph;
using SIL.Cog.ViewModels;

namespace SIL.Cog.Services
{
	public interface IExportService
	{
		bool ExportSimilarityMatrix(object ownerViewModel, CogProject project, SimilarityMetric similarityMetric);
		bool ExportWordLists(object ownerViewModel, CogProject project);
		bool ExportCognateSets(object ownerViewModel, CogProject project);
		bool ExportVarietyPair(object ownerViewModel, CogProject project, VarietyPair varietyPair);

		bool ExportCurrentHierarchicalGraph(object ownerViewModel, HierarchicalGraphType type);
		bool ExportHierarchicalGraph(object ownerViewModel, IHierarchicalBidirectionalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge> graph, HierarchicalGraphType graphType);
		bool ExportCurrentNetworkGraph(object ownerViewModel);
		bool ExportNetworkGraph(object ownerViewModel, IBidirectionalGraph<NetworkGraphVertex, NetworkGraphEdge> graph, double scoreFilter);
		bool ExportCurrentMap(object ownerViewModel);
	}
}
