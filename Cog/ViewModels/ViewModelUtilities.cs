using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using GraphSharp;
using QuickGraph;
using SIL.Cog.Clusterers;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.ViewModels
{
	public static class ViewModelUtilities
	{
		public static IHierarchicalBidirectionalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge> GenerateHierarchicalGraph(CogProject project, ClusteringMethod clusteringMethod,
			SimilarityMetric similarityMetric)
		{
			IEnumerable<Cluster<Variety>> clusters = null;
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
					clusters = upgma.GenerateClusters(project.Varieties);
					break;

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
					clusters = nj.GenerateClusters(project.Varieties);
					break;
			}
			Debug.Assert(clusters != null);
			var graph = new HierarchicalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge>();
			Cluster<Variety>[] clusterArray = clusters.ToArray();
			var root = new HierarchicalGraphVertex(0);
			IEnumerable<Tuple<Cluster<Variety>, double>> clusterLengths;
			IEnumerable<Variety> varieties;
			if (clusterArray.Length == 1)
			{
				Cluster<Variety> rootCluster = clusterArray[0];
				clusterLengths = rootCluster.Children.Select(c => Tuple.Create(c, rootCluster.Children.GetLength(c)));
				varieties = rootCluster.DataObjects;
			}
			else
			{
				clusterLengths = clusterArray.Select(c => Tuple.Create(c, 0.0));
				varieties = clusterArray.SelectMany(c => c.DataObjects);
			}
			graph.AddVertex(root);
			GenerateVertices(graph, root, clusterLengths, varieties);

			return graph;
		}

		private static void GenerateVertices(HierarchicalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge> graph, HierarchicalGraphVertex vertex,
			IEnumerable<Tuple<Cluster<Variety>, double>> children, IEnumerable<Variety> varieties)
		{
			var childVarieties = new HashSet<Variety>();
			foreach (Tuple<Cluster<Variety>, double> child in children)
			{
				Cluster<Variety> childCluster = child.Item1;
				double length = child.Item2;
				HierarchicalGraphVertex newVertex;
				if (childCluster.IsLeaf && childCluster.DataObjects.Count == 1)
				{
					newVertex = new HierarchicalGraphVertex(childCluster.DataObjects.First(), vertex.Depth + length);
					graph.AddVertex(newVertex);
				}
				else
				{
					newVertex = new HierarchicalGraphVertex(vertex.Depth + length);
					graph.AddVertex(newVertex);
					GenerateVertices(graph, newVertex, childCluster.Children.Select(c => Tuple.Create(c, childCluster.Children.GetLength(c))), childCluster.DataObjects);
				}
				graph.AddEdge(new HierarchicalGraphEdge(vertex, newVertex, EdgeTypes.Hierarchical, child.Item2));
				childVarieties.UnionWith(childCluster.DataObjects);
			}

			foreach (Variety variety in varieties.Except(childVarieties))
			{
				var vm = new HierarchicalGraphVertex(variety, 0);
				graph.AddVertex(vm);
				graph.AddEdge(new HierarchicalGraphEdge(vertex, vm, EdgeTypes.Hierarchical, 0));
			}
		}

		public static IBidirectionalGraph<NetworkGraphVertex, NetworkGraphEdge> GenerateNetworkGraph(CogProject project, SimilarityMetric similarityMetric)
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

		public static string GetFeatureStructureString(FeatureStruct fs)
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
	}
}
