using QuickGraph;
using SIL.Cog.Applications.ViewModels;

namespace SIL.Cog.Applications.Services
{
	public interface IGraphService
	{
		IBidirectionalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge> GenerateHierarchicalGraph(HierarchicalGraphType graphType,
			ClusteringMethod clusteringMethod, SimilarityMetric similarityMetric);

		IBidirectionalGraph<NetworkGraphVertex, NetworkGraphEdge> GenerateNetworkGraph(SimilarityMetric similarityMetric);
	}
}
