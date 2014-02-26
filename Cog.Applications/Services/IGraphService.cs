using System.Collections.Generic;
using QuickGraph;
using SIL.Cog.Applications.ViewModels;
using SIL.Cog.Domain;

namespace SIL.Cog.Applications.Services
{
	public interface IGraphService
	{
		IBidirectionalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge> GenerateHierarchicalGraph(HierarchicalGraphType graphType,
			ClusteringMethod clusteringMethod, SimilarityMetric similarityMetric);

		IBidirectionalGraph<NetworkGraphVertex, NetworkGraphEdge> GenerateNetworkGraph(SimilarityMetric similarityMetric);

		IBidirectionalGraph<GridVertex, GlobalCorrespondenceEdge> GenerateGlobalCorrespondencesGraph(SyllablePosition syllablePosition);
		IBidirectionalGraph<GridVertex, GlobalCorrespondenceEdge> GenerateGlobalCorrespondencesGraph(SyllablePosition syllablePosition, IEnumerable<Variety> varieties);
	}
}
