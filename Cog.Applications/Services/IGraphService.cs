using System.Collections.Generic;
using QuickGraph;
using SIL.Cog.Applications.GraphAlgorithms;
using SIL.Cog.Applications.ViewModels;
using SIL.Cog.Domain;

namespace SIL.Cog.Applications.Services
{
	public interface IGraphService
	{
		IBidirectionalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge> GenerateHierarchicalGraph(HierarchicalGraphType graphType,
			ClusteringMethod clusteringMethod, SimilarityMetric similarityMetric);

		IBidirectionalGraph<NetworkGraphVertex, NetworkGraphEdge> GenerateNetworkGraph(SimilarityMetric similarityMetric);

		IBidirectionalGraph<GridVertex, GlobalCorrespondenceEdge> GenerateGlobalCorrespondencesGraph(ViewModelSyllablePosition syllablePosition);
		IBidirectionalGraph<GridVertex, GlobalCorrespondenceEdge> GenerateGlobalCorrespondencesGraph(ViewModelSyllablePosition syllablePosition, IEnumerable<Variety> varieties);
	}
}
