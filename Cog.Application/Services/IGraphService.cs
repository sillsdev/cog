using System.Collections.Generic;
using QuickGraph;
using SIL.Cog.Application.ViewModels;
using SIL.Cog.Domain;

namespace SIL.Cog.Application.Services
{
	public interface IGraphService
	{
		IBidirectionalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge> GenerateHierarchicalGraph(HierarchicalGraphType graphType,
			ClusteringMethod clusteringMethod, SimilarityMetric similarityMetric);

		IBidirectionalGraph<NetworkGraphVertex, NetworkGraphEdge> GenerateNetworkGraph(SimilarityMetric similarityMetric);

		IBidirectionalGraph<GlobalCorrespondencesGraphVertex, GlobalCorrespondencesGraphEdge> GenerateGlobalCorrespondencesGraph(SyllablePosition syllablePosition);
		IBidirectionalGraph<GlobalCorrespondencesGraphVertex, GlobalCorrespondencesGraphEdge> GenerateGlobalCorrespondencesGraph(SyllablePosition syllablePosition, IEnumerable<Variety> varieties);
	}
}
