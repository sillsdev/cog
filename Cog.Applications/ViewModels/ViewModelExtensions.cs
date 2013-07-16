using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph;
using QuickGraph.Algorithms;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Clusterers;
using SIL.Cog.Domain.Components;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Applications.ViewModels
{
	public static class ViewModelExtensions
	{
		public static IBidirectionalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge> GenerateHierarchicalGraph(this CogProject project, HierarchicalGraphType graphType,
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
					IBidirectionalGraph<Cluster<Variety>, ClusterEdge<Variety>> upgmaTree = upgma.GenerateClusters(project.Varieties);
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
					IUndirectedGraph<Cluster<Variety>, ClusterEdge<Variety>> njTree = nj.GenerateClusters(project.Varieties);
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

		public static IBidirectionalGraph<NetworkGraphVertex, NetworkGraphEdge> GenerateNetworkGraph(this CogProject project, SimilarityMetric similarityMetric)
		{
			var graph = new BidirectionalGraph<NetworkGraphVertex, NetworkGraphEdge>();
			var dict = new Dictionary<Variety, NetworkGraphVertex>();
			foreach (Variety variety in project.Varieties)
			{
				var vertex = new NetworkGraphVertex(variety);
				graph.AddVertex(vertex);
				dict[variety] = vertex;
			}
			foreach (VarietyPair pair in project.VarietyPairs)
				graph.AddEdge(new NetworkGraphEdge(dict[pair.Variety1], dict[pair.Variety2], pair, similarityMetric));

			return graph;
		}

		public static string GetString(this FeatureStruct fs)
		{
			var sb = new StringBuilder();
			sb.Append("[");
			bool firstFeature = true;
			foreach (SymbolicFeature feature in fs.Features.Where(f => !CogFeatureSystem.Instance.ContainsFeature(f)))
			{
				if (!firstFeature)
					sb.Append(",");
				sb.Append(feature.Description);
				sb.Append(":");
				SymbolicFeatureValue fv = fs.GetValue(feature);
				FeatureSymbol[] symbols = fv.Values.ToArray();
				if (symbols.Length > 1)
					sb.Append("{");
				bool firstSymbol = true;
				foreach (FeatureSymbol symbol in symbols)
				{
					if (!firstSymbol)
						sb.Append(",");
					sb.Append(symbol.Description);
					firstSymbol = false;
				}
				if (symbols.Length > 1)
					sb.Append("}");
				firstFeature = false;
			}
			sb.Append("]");
			return sb.ToString();
		}

		public static IEnumerable<IProcessor<VarietyPair>> GetComparisonProcessors(this CogProject project)
		{
			var processors = new List<IProcessor<VarietyPair>> {new WordPairGenerator(project, "primary")};
			IProcessor<VarietyPair> similarSegmentIdentifier;
			if (project.VarietyPairProcessors.TryGetValue("similarSegmentIdentifier", out similarSegmentIdentifier))
				processors.Add(similarSegmentIdentifier);
			processors.Add(project.VarietyPairProcessors["soundChangeInducer"]);
			return processors;
		}

		public static IEnumerable<IProcessor<Variety>> GetVarietyInitProcessors(this CogProject project)
		{
			var processors = new List<IProcessor<Variety>> {new VarietySegmenter(project)};
			IProcessor<Variety> syllabifier;
			if (project.VarietyProcessors.TryGetValue("syllabifier", out syllabifier))
				processors.Add(syllabifier);
			processors.Add(new SegmentDistributionCalculator());
			return processors;
		}

		public static IEnumerable<IProcessor<Variety>> GetStemmingProcessors(this CogProject project, SpanFactory<ShapeNode> spanFactory, StemmingMethod method)
		{
			var processors = new List<IProcessor<Variety>> {new AffixStripper(project)};
			if (method != StemmingMethod.Manual)
				processors.Add(project.VarietyProcessors["affixIdentifier"]);
			processors.Add(new Stemmer(spanFactory, project));
			IProcessor<Variety> syllabifier;
			if (project.VarietyProcessors.TryGetValue("syllabifier", out syllabifier))
				processors.Add(syllabifier);
			processors.Add(new SegmentDistributionCalculator());
			return processors;
		}
	}
}
