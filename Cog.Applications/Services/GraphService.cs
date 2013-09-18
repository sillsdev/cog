using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using QuickGraph;
using QuickGraph.Algorithms;
using SIL.Cog.Applications.GraphAlgorithms;
using SIL.Cog.Applications.ViewModels;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Clusterers;
using SIL.Collections;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Applications.Services
{
	public class GraphService : IGraphService
	{
		private static readonly Dictionary<string, int> VowelHeightLookup = new Dictionary<string, int>
			{
				{"close", 1},
				{"near-close", 2},
				{"close-mid", 3},
				{"mid", 4},
				{"open-mid", 5},
				{"near-open", 6},
				{"open", 7}
			};

		private static readonly Dictionary<string, int> VowelBacknessLookup = new Dictionary<string, int>
			{
				{"front", 1},
				{"near-front", 4},
				{"central", 7},
				{"near-back", 10},
				{"back", 13}
			};

		private static readonly Dictionary<string, int> ConsonantPlaceLookup = new Dictionary<string, int>
			{
				{"bilabial", 1},
				{"labiodental", 4},
				{"dental", 7},
				{"alveolar", 10},
				{"palato-alveolar", 13},
				{"retroflex", 16},
				{"palatal", 19},
				{"velar", 22},
				{"uvular", 25},
				{"pharyngeal", 28},
				{"glottal", 31}
			};

		private static readonly Dictionary<string, int> ConsonantMannerLookup = new Dictionary<string, int>
			{
				{"stop", 2},
				{"affricate", 3},
				{"fricative", 4},
				{"approximant", 5},
				{"flap", 6},
				{"trill", 7},
			};

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

		public IBidirectionalGraph<GridVertex, GlobalCorrespondenceEdge> GenerateGlobalCorrespondencesGraph(ViewModelSyllablePosition syllablePosition)
		{
			CogProject project = _projectService.Project;
			var graph = new BidirectionalGraph<GridVertex, GlobalCorrespondenceEdge>();

			var vertices = new Dictionary<Tuple<int, int>, GlobalSegmentVertex>();
			var edges = new Dictionary<UnorderedTuple<Tuple<int, int>, Tuple<int, int>>, GlobalCorrespondenceEdge>();
			int maxFreq = 0;
			if (syllablePosition == ViewModelSyllablePosition.Nucleus)
			{
				graph.AddVertexRange(new []
					{
						new HeaderGridVertex("Front") {Row = 0, Column = 1, ColumnSpan = 3},
						new HeaderGridVertex("Central") {Row = 0, Column = 7, ColumnSpan = 3},
						new HeaderGridVertex("Back") {Row = 0, Column = 13, ColumnSpan = 3},

						new HeaderGridVertex("Close") {Row = 1, Column = 0, HorizontalAlignment = GridHorizontalAlignment.Left},
						new HeaderGridVertex("Close-mid") {Row = 3, Column = 0, HorizontalAlignment = GridHorizontalAlignment.Left},
						new HeaderGridVertex("Open-mid") {Row = 5, Column = 0, HorizontalAlignment = GridHorizontalAlignment.Left},
						new HeaderGridVertex("Open") {Row = 7, Column = 0, HorizontalAlignment = GridHorizontalAlignment.Left}
					});

				foreach (GlobalSoundCorrespondence corr in project.GlobalSoundCorrespondenceCollections[SyllablePosition.Nucleus])
				{
					GlobalSegmentVertex vertex1, vertex2;
					if (GetVowel(vertices, corr.Segment1, out vertex1) && GetVowel(vertices, corr.Segment2, out vertex2) && vertex1 != vertex2)
					{
						int freq = AddEdge(edges, corr, vertex1, vertex2);
						maxFreq = Math.Max(freq, maxFreq);
					}
				}
			}
			else
			{
				graph.AddVertexRange(new[]
					{
						new HeaderGridVertex("Bilabial") {Row = 0, Column = 1, ColumnSpan = 3},
						new HeaderGridVertex("Labiodental") {Row = 0, Column = 4, ColumnSpan = 3},
						new HeaderGridVertex("Dental") {Row = 0, Column = 7, ColumnSpan = 3},
						new HeaderGridVertex("Alveolar") {Row = 0, Column = 10, ColumnSpan = 3},
						new HeaderGridVertex("Postalveolar") {Row = 0, Column = 13, ColumnSpan = 3},
						new HeaderGridVertex("Retroflex") {Row = 0, Column = 16, ColumnSpan = 3},
						new HeaderGridVertex("Palatal") {Row = 0, Column = 19, ColumnSpan = 3},
						new HeaderGridVertex("Velar") {Row = 0, Column = 22, ColumnSpan = 3},
						new HeaderGridVertex("Uvular") {Row = 0, Column = 25, ColumnSpan = 3},
						new HeaderGridVertex("Pharyngeal") {Row = 0, Column = 28, ColumnSpan = 3},
						new HeaderGridVertex("Glottal") {Row = 0, Column = 31, ColumnSpan = 3},

						new HeaderGridVertex("Nasal") {Row = 1, Column = 0, HorizontalAlignment = GridHorizontalAlignment.Left},
						new HeaderGridVertex("Stop") {Row = 2, Column = 0, HorizontalAlignment = GridHorizontalAlignment.Left},
						new HeaderGridVertex("Affricate") {Row = 3, Column = 0, HorizontalAlignment = GridHorizontalAlignment.Left},
						new HeaderGridVertex("Fricative") {Row = 4, Column = 0, HorizontalAlignment = GridHorizontalAlignment.Left},
						new HeaderGridVertex("Approximant") {Row = 5, Column = 0, HorizontalAlignment = GridHorizontalAlignment.Left},
						new HeaderGridVertex("Flap or tap") {Row = 6, Column = 0, HorizontalAlignment = GridHorizontalAlignment.Left},
						new HeaderGridVertex("Trill") {Row = 7, Column = 0, HorizontalAlignment = GridHorizontalAlignment.Left},
						new HeaderGridVertex("Lateral fricative") {Row = 8, Column = 0, HorizontalAlignment = GridHorizontalAlignment.Left},
						new HeaderGridVertex("Lateral approximant") {Row = 9, Column = 0, HorizontalAlignment = GridHorizontalAlignment.Left}
					});

				GlobalSoundCorrespondenceCollection corrs = null;
				switch (syllablePosition)
				{
					case ViewModelSyllablePosition.Onset:
						corrs = project.GlobalSoundCorrespondenceCollections[SyllablePosition.Onset];
						break;
					case ViewModelSyllablePosition.Coda:
						corrs = project.GlobalSoundCorrespondenceCollections[SyllablePosition.Coda];
						break;
				}
				Debug.Assert(corrs != null);
				foreach (GlobalSoundCorrespondence corr in corrs)
				{
					GlobalSegmentVertex vertex1, vertex2;
					if (GetConsonant(vertices, corr.Segment1, out vertex1) && GetConsonant(vertices, corr.Segment2, out vertex2) && vertex1 != vertex2)
					{
						int freq = AddEdge(edges, corr, vertex1, vertex2);
						maxFreq = Math.Max(freq, maxFreq);
					}
				}
			}

			graph.AddVertexRange(vertices.Values);
			foreach (GlobalCorrespondenceEdge edge in edges.Values)
			{
				edge.NormalizedFrequency = (double) edge.Frequency / maxFreq;
				graph.AddEdge(edge);
			}

			return graph;
		}

		private static int AddEdge(Dictionary<UnorderedTuple<Tuple<int, int>, Tuple<int, int>>, GlobalCorrespondenceEdge> edges, GlobalSoundCorrespondence corr,
			GlobalSegmentVertex vertex1, GlobalSegmentVertex vertex2)
		{
			Tuple<int, int> key1 = Tuple.Create(vertex1.Row, vertex1.Column);
			Tuple<int, int> key2 = Tuple.Create(vertex2.Row, vertex2.Column);
			GlobalCorrespondenceEdge edge = edges.GetValue(UnorderedTuple.Create(key1, key2), () => new GlobalCorrespondenceEdge(vertex1, vertex2));
			edge.Frequency += corr.Frequency;
			edge.DomainWordPairs.AddRange(corr.WordPairs);
			return edge.Frequency;
		}

		private static bool GetConsonant(Dictionary<Tuple<int, int>, GlobalSegmentVertex> vertices, Segment consonant, out GlobalSegmentVertex vertex)
		{
			if (consonant.IsComplex)
			{
				vertex = null;
				return false;
			}

			var placeSymbol = (FeatureSymbol) consonant.FeatureStruct.GetValue<SymbolicFeatureValue>("place");
			var mannerSymbol = (FeatureSymbol) consonant.FeatureStruct.GetValue<SymbolicFeatureValue>("manner");
			var voiceSymbol = (FeatureSymbol) consonant.FeatureStruct.GetValue<SymbolicFeatureValue>("voice");
			var nasalSymbol = (FeatureSymbol) consonant.FeatureStruct.GetValue<SymbolicFeatureValue>("nasal");

			int column;
			if (!ConsonantPlaceLookup.TryGetValue(placeSymbol.ID, out column))
			{
				vertex = null;
				return false;
			}

			int row;
			if (nasalSymbol.ID == "nasal+")
			{
				row = 1;
			}
			else if (ConsonantMannerLookup.TryGetValue(mannerSymbol.ID, out row))
			{
				var lateralSymbol = (FeatureSymbol) consonant.FeatureStruct.GetValue<SymbolicFeatureValue>("lateral");
				if (lateralSymbol.ID == "lateral+")
					row += 4;
			}
			else
			{
				vertex = null;
				return false;
			}

			var alignment = GridHorizontalAlignment.Right;
			if (voiceSymbol.ID == "voice+")
			{
				column += 2;
				alignment = GridHorizontalAlignment.Left;
			}

			vertex = vertices.GetValue(Tuple.Create(row, column), () => new GlobalSegmentVertex {Row = row, Column = column, HorizontalAlignment = alignment} );
			vertex.StrReps.Add(consonant.StrRep);
			return true;
		}

		private static bool GetVowel(Dictionary<Tuple<int, int>, GlobalSegmentVertex> vertices, Segment vowel, out GlobalSegmentVertex vertex)
		{
			if (vowel.IsComplex)
			{
				vertex = null;
				return false;
			}

			var heightSymbol = (FeatureSymbol) vowel.FeatureStruct.GetValue<SymbolicFeatureValue>("height");
			var backnessSymbol = (FeatureSymbol) vowel.FeatureStruct.GetValue<SymbolicFeatureValue>("backness");
			var roundSymbol = (FeatureSymbol) vowel.FeatureStruct.GetValue<SymbolicFeatureValue>("round");

			int row = VowelHeightLookup[heightSymbol.ID];
			int column = VowelBacknessLookup[backnessSymbol.ID];

			var alignment = GridHorizontalAlignment.Right;
			if (roundSymbol.ID == "round+")
			{
				column += 2;
				alignment = GridHorizontalAlignment.Left;
			}

			vertex = vertices.GetValue(Tuple.Create(row, column), () => new GlobalSegmentVertex {Row = row, Column = column, HorizontalAlignment = alignment});
			vertex.StrReps.Add(vowel.StrRep);
			return true;
		}
	}
}
