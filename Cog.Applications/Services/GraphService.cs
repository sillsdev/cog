using System;
using System.Collections.Generic;
using System.Linq;
using QuickGraph;
using QuickGraph.Algorithms;
using SIL.Cog.Applications.ViewModels;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Clusterers;

namespace SIL.Cog.Applications.Services
{
	public class GraphService : IGraphService
	{
		private readonly IProjectService _projectService;

		public GraphService(IProjectService projectService)
		{
			_projectService = projectService;
		}

		public IBidirectionalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge> GenerateHierarchicalGraph(HierarchicalGraphType graphType,
			ClusteringMethod clusteringMethod, SimilarityMetric similarityMetric)
		{
			switch (clusteringMethod)
			{
				case ClusteringMethod.Upgma:
					Func<Variety, Variety, double> upgmaGetDistance = null;
					switch (similarityMetric)
					{
						case SimilarityMetric.Lexical:
							upgmaGetDistance = (v1, v2) => 1.0 - v1.VarietyPairs[v2].LexicalSimilarityScore;
							break;
						case SimilarityMetric.Phonetic:
							upgmaGetDistance = (v1, v2) => 1.0 - v1.VarietyPairs[v2].PhoneticSimilarityScore;
							break;
					}

					var upgma = new UpgmaClusterer<Variety>(upgmaGetDistance);
					IBidirectionalGraph<Cluster<Variety>, ClusterEdge<Variety>> upgmaTree = upgma.GenerateClusters(_projectService.Project.Varieties);
					return BuildHierarchicalGraph(upgmaTree);

				case ClusteringMethod.NeighborJoining:
					Func<Variety, Variety, double> njGetDistance = null;
					switch (similarityMetric)
					{
						case SimilarityMetric.Lexical:
							njGetDistance = (v1, v2) => 1.0 - v1.VarietyPairs[v2].LexicalSimilarityScore;
							break;
						case SimilarityMetric.Phonetic:
							njGetDistance = (v1, v2) => 1.0 - v1.VarietyPairs[v2].PhoneticSimilarityScore;
							break;
					}
					var nj = new NeighborJoiningClusterer<Variety>(njGetDistance);
					IUndirectedGraph<Cluster<Variety>, ClusterEdge<Variety>> njTree = nj.GenerateClusters(_projectService.Project.Varieties);
					switch (graphType)
					{
						case HierarchicalGraphType.Dendrogram:
							IBidirectionalGraph<Cluster<Variety>, ClusterEdge<Variety>> rootedTree = njTree.ToRootedTree();
							return BuildHierarchicalGraph(rootedTree);

						case HierarchicalGraphType.Tree:
							return BuildHierarchicalGraph(njTree);
					}
					break;
			}

			return null;
		}

		private static IBidirectionalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge> BuildHierarchicalGraph(IBidirectionalGraph<Cluster<Variety>, ClusterEdge<Variety>> tree)
		{
			var graph = new BidirectionalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge>();
			var root = new HierarchicalGraphVertex(0);
			graph.AddVertex(root);
			GenerateHierarchicalVertices(graph, root, tree, tree.Roots().First());
			return graph;
		}

		private static void GenerateHierarchicalVertices(BidirectionalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge> graph, HierarchicalGraphVertex vertex,
			IBidirectionalGraph<Cluster<Variety>, ClusterEdge<Variety>> tree, Cluster<Variety> cluster)
		{
			foreach (ClusterEdge<Variety> edge in tree.OutEdges(cluster))
			{
				double depth = vertex.Depth + edge.Length;
				var newVertex = edge.Target.DataObjects.Count == 1 ? new HierarchicalGraphVertex(edge.Target.DataObjects.First(), depth) : new HierarchicalGraphVertex(depth);
				graph.AddVertex(newVertex);
				graph.AddEdge(new HierarchicalGraphEdge(vertex, newVertex, edge.Length));
				GenerateHierarchicalVertices(graph, newVertex, tree, edge.Target);
			}
		}

		private static IBidirectionalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge> BuildHierarchicalGraph(IUndirectedGraph<Cluster<Variety>, ClusterEdge<Variety>> tree)
		{
			var graph = new BidirectionalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge>();
			var root = new HierarchicalGraphVertex(0);
			graph.AddVertex(root);
			GenerateHierarchicalVertices(graph, root, tree, null, tree.GetCenter());
			return graph;
		}

		private static void GenerateHierarchicalVertices(BidirectionalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge> graph, HierarchicalGraphVertex vertex,
			IUndirectedGraph<Cluster<Variety>, ClusterEdge<Variety>> tree, Cluster<Variety> parent, Cluster<Variety> cluster)
		{
			foreach (ClusterEdge<Variety> edge in tree.AdjacentEdges(cluster).Where(e => e.GetOtherVertex(cluster) != parent))
			{
				Cluster<Variety> target = edge.GetOtherVertex(cluster);
				double depth = vertex.Depth + edge.Length;
				var newVertex = target.DataObjects.Count == 1 ? new HierarchicalGraphVertex(target.DataObjects.First(), depth) : new HierarchicalGraphVertex(depth);
				graph.AddVertex(newVertex);
				graph.AddEdge(new HierarchicalGraphEdge(vertex, newVertex, edge.Length));
				GenerateHierarchicalVertices(graph, newVertex, tree, cluster, target);
			}
		}

		public IBidirectionalGraph<NetworkGraphVertex, NetworkGraphEdge> GenerateNetworkGraph(SimilarityMetric similarityMetric)
		{
			var graph = new BidirectionalGraph<NetworkGraphVertex, NetworkGraphEdge>();
			var dict = new Dictionary<Variety, NetworkGraphVertex>();
			foreach (Variety variety in _projectService.Project.Varieties)
			{
				var vertex = new NetworkGraphVertex(variety);
				graph.AddVertex(vertex);
				dict[variety] = vertex;
			}
			foreach (VarietyPair pair in _projectService.Project.VarietyPairs)
				graph.AddEdge(new NetworkGraphEdge(dict[pair.Variety1], dict[pair.Variety2], pair, similarityMetric));

			return graph;
		}

	}
}
