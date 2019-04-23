using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using QuickGraph;
using QuickGraph.Algorithms;
using SIL.Cog.Application.ViewModels;
using SIL.Cog.Domain;
using SIL.Collections;
using SIL.Machine.Clusterers;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Application.Services
{
	public class GraphService : IGraphService
	{
		private static readonly Dictionary<string, VowelHeight> VowelHeightLookup = new Dictionary<string, VowelHeight>
			{
				{"close", VowelHeight.Close},
				{"near-close", VowelHeight.NearClose},
				{"close-mid", VowelHeight.CloseMid},
				{"mid", VowelHeight.Mid},
				{"open-mid", VowelHeight.OpenMid},
				{"near-open", VowelHeight.NearOpen},
				{"open", VowelHeight.Open}
			};

		private static readonly Dictionary<string, VowelBackness> VowelBacknessLookup = new Dictionary<string, VowelBackness>
			{
				{"front", VowelBackness.Front},
				{"near-front", VowelBackness.NearFront},
				{"central", VowelBackness.Central},
				{"near-back", VowelBackness.NearBack},
				{"back", VowelBackness.Back}
			};

		private static readonly Dictionary<string, ConsonantPlace> ConsonantPlaceLookup = new Dictionary<string, ConsonantPlace>
			{
				{"bilabial", ConsonantPlace.Bilabial},
				{"labiodental", ConsonantPlace.Labiodental},
				{"dental", ConsonantPlace.Dental},
				{"alveolar", ConsonantPlace.Alveolar},
				{"palato-alveolar", ConsonantPlace.Postalveolar},
				{"alveolo-palatal", ConsonantPlace.Postalveolar},
				{"retroflex", ConsonantPlace.Retroflex},
				{"palatal", ConsonantPlace.Palatal},
				{"velar", ConsonantPlace.Velar},
				{"uvular", ConsonantPlace.Uvular},
				{"pharyngeal", ConsonantPlace.Pharyngeal},
				{"glottal", ConsonantPlace.Glottal}
			};

		private static readonly Dictionary<string, ConsonantManner> ConsonantMannerLookup = new Dictionary<string, ConsonantManner>
			{
				{"stop", ConsonantManner.Stop},
				{"affricate", ConsonantManner.Affricate},
				{"fricative", ConsonantManner.Fricative},
				{"approximant", ConsonantManner.Approximant},
				{"flap", ConsonantManner.FlapOrTap},
				{"trill", ConsonantManner.Trill},
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
			if (tree.VertexCount > 2)
			{
				GenerateHierarchicalVertices(graph, root, tree, null, tree.GetCenter());
			}
			else
			{
				foreach (Cluster<Variety> cluster in tree.Vertices)
					GenerateHierarchicalVertices(graph, root, tree, null, cluster);
			}
			
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

		public IBidirectionalGraph<GlobalCorrespondencesGraphVertex, GlobalCorrespondencesGraphEdge> GenerateGlobalCorrespondencesGraph(SyllablePosition syllablePosition)
		{
			return GenerateGlobalCorrespondencesGraph(syllablePosition, _projectService.Project.Varieties);
		}

		public IBidirectionalGraph<GlobalCorrespondencesGraphVertex, GlobalCorrespondencesGraphEdge> GenerateGlobalCorrespondencesGraph(SyllablePosition syllablePosition, IEnumerable<Variety> varieties)
		{
			var varietiesSet = new HashSet<Variety>(varieties);
			CogProject project = _projectService.Project;
			var graph = new BidirectionalGraph<GlobalCorrespondencesGraphVertex, GlobalCorrespondencesGraphEdge>();
			var vertices = new Dictionary<object, GlobalSegmentVertex>();
			var edges = new Dictionary<UnorderedTuple<object, object>, GlobalCorrespondencesGraphEdge>();
			int maxFreq = 0;
			if (syllablePosition == SyllablePosition.Nucleus)
			{
				graph.AddVertexRange(new GlobalCorrespondencesGraphVertex[]
					{
						new VowelBacknessVertex(VowelBackness.Front),
						new VowelBacknessVertex(VowelBackness.Central),
						new VowelBacknessVertex(VowelBackness.Back),

						new VowelHeightVertex(VowelHeight.Close),
						new VowelHeightVertex(VowelHeight.CloseMid),
						new VowelHeightVertex(VowelHeight.OpenMid),
						new VowelHeightVertex(VowelHeight.Open)
					});

				foreach (VarietyPair vp in project.VarietyPairs.Where(vp => varietiesSet.Contains(vp.Variety1) && varietiesSet.Contains(vp.Variety2)))
				{
					foreach (SoundCorrespondence corr in vp.CognateSoundCorrespondencesByPosition[CogFeatureSystem.Nucleus])
					{
						VowelHeight height1, height2;
						VowelBackness backness1, backness2;
						bool round1, round2;
						if (GetVowelInfo(corr.Segment1, out height1, out backness1, out round1) && GetVowelInfo(corr.Segment2, out height2, out backness2, out round2)
							&& (height1 != height2 || backness1 != backness2 || round1 != round2))
						{
							Tuple<VowelHeight, VowelBackness, bool> key1 = Tuple.Create(height1, backness1, round1);
							GlobalSegmentVertex vertex1 = vertices.GetValue(key1, () => new GlobalVowelVertex(height1, backness1, round1));
							vertex1.StrReps.Add(corr.Segment1.StrRep);
							Tuple<VowelHeight, VowelBackness, bool> key2 = Tuple.Create(height2, backness2, round2);
							GlobalSegmentVertex vertex2 = vertices.GetValue(key2, () => new GlobalVowelVertex(height2, backness2, round2));
							vertex2.StrReps.Add(corr.Segment2.StrRep);
							int freq = AddEdge(edges, corr, key1, vertex1, key2, vertex2);
							maxFreq = Math.Max(freq, maxFreq);
						}
					}
				}
			}
			else
			{
				graph.AddVertexRange(new GlobalCorrespondencesGraphVertex[]
					{
						new ConsonantPlaceVertex(ConsonantPlace.Bilabial),
						new ConsonantPlaceVertex(ConsonantPlace.Labiodental), 
						new ConsonantPlaceVertex(ConsonantPlace.Dental),
						new ConsonantPlaceVertex(ConsonantPlace.Alveolar),
						new ConsonantPlaceVertex(ConsonantPlace.Postalveolar),
						new ConsonantPlaceVertex(ConsonantPlace.Retroflex),
						new ConsonantPlaceVertex(ConsonantPlace.Palatal),
						new ConsonantPlaceVertex(ConsonantPlace.Velar),
						new ConsonantPlaceVertex(ConsonantPlace.Uvular),
						new ConsonantPlaceVertex(ConsonantPlace.Pharyngeal), 
						new ConsonantPlaceVertex(ConsonantPlace.Glottal),

						new ConsonantMannerVertex(ConsonantManner.Nasal),
						new ConsonantMannerVertex(ConsonantManner.Stop),
						new ConsonantMannerVertex(ConsonantManner.Affricate),
						new ConsonantMannerVertex(ConsonantManner.Fricative),
						new ConsonantMannerVertex(ConsonantManner.Approximant),
						new ConsonantMannerVertex(ConsonantManner.FlapOrTap),
						new ConsonantMannerVertex(ConsonantManner.Trill),
						new ConsonantMannerVertex(ConsonantManner.LateralFricative),
						new ConsonantMannerVertex(ConsonantManner.LateralApproximant)
					});

				foreach (VarietyPair vp in project.VarietyPairs.Where(vp => varietiesSet.Contains(vp.Variety1) && varietiesSet.Contains(vp.Variety2)))
				{
					SoundCorrespondenceCollection corrs = null;
					switch (syllablePosition)
					{
						case SyllablePosition.Onset:
							corrs = vp.CognateSoundCorrespondencesByPosition[CogFeatureSystem.Onset];
							break;
						case SyllablePosition.Coda:
							corrs = vp.CognateSoundCorrespondencesByPosition[CogFeatureSystem.Coda];
							break;
					}
					Debug.Assert(corrs != null);
					foreach (SoundCorrespondence corr in corrs)
					{
						ConsonantPlace place1, place2;
						ConsonantManner manner1, manner2;
						bool voiced1, voiced2;
						if (GetConsonantPosition(corr.Segment1, out place1, out manner1, out voiced1) && GetConsonantPosition(corr.Segment2, out place2, out manner2, out voiced2)
							&& (place1 != place2 || manner1 != manner2 || voiced1 != voiced2))
						{
							Tuple<ConsonantPlace, ConsonantManner, bool> key1 = Tuple.Create(place1, manner1, voiced1);
							GlobalSegmentVertex vertex1 = vertices.GetValue(key1, () => new GlobalConsonantVertex(place1, manner1, voiced1));
							vertex1.StrReps.Add(corr.Segment1.StrRep);
							Tuple<ConsonantPlace, ConsonantManner, bool> key2 = Tuple.Create(place2, manner2, voiced2);
							GlobalSegmentVertex vertex2 = vertices.GetValue(key2, () => new GlobalConsonantVertex(place2, manner2, voiced2));
							vertex2.StrReps.Add(corr.Segment2.StrRep);

							int freq = AddEdge(edges, corr, key1, vertex1, key2, vertex2);
							maxFreq = Math.Max(freq, maxFreq);
						}
					}
				}
			}

			graph.AddVertexRange(vertices.Values);
			foreach (GlobalCorrespondencesGraphEdge edge in edges.Values)
			{
				edge.NormalizedFrequency = (double) edge.Frequency / maxFreq;
				graph.AddEdge(edge);
			}

			return graph;
		}

		private static int AddEdge(Dictionary<UnorderedTuple<object, object>, GlobalCorrespondencesGraphEdge> edges, SoundCorrespondence corr,
			object key1, GlobalSegmentVertex vertex1, object key2, GlobalSegmentVertex vertex2)
		{
			GlobalCorrespondencesGraphEdge edge = edges.GetValue(UnorderedTuple.Create(key1, key2), () => new GlobalCorrespondencesGraphEdge(vertex1, vertex2));
			edge.Frequency += corr.Frequency;
			edge.DomainWordPairs.AddRange(corr.WordPairs);
			return edge.Frequency;
		}

		private static bool GetConsonantPosition(Segment consonant, out ConsonantPlace place, out ConsonantManner manner, out bool voiced)
		{
			FeatureStruct fs = consonant.FeatureStruct;
			if (consonant.IsComplex)
				fs = consonant.FeatureStruct.GetValue<FeatureStruct>(CogFeatureSystem.First);

			var placeSymbol = (FeatureSymbol) fs.GetValue<SymbolicFeatureValue>("place");
			var mannerSymbol = (FeatureSymbol) fs.GetValue<SymbolicFeatureValue>("manner");
			var voiceSymbol = (FeatureSymbol) fs.GetValue<SymbolicFeatureValue>("voice");
			var nasalSymbol = (FeatureSymbol) fs.GetValue<SymbolicFeatureValue>("nasal");

			manner = default(ConsonantManner);
			voiced = false;

			if (!ConsonantPlaceLookup.TryGetValue(placeSymbol.ID, out place))
				return false;

			if (nasalSymbol.ID == "nasal+")
			{
				manner = ConsonantManner.Nasal;
			}
			else if (ConsonantMannerLookup.TryGetValue(mannerSymbol.ID, out manner))
			{
				var lateralSymbol = (FeatureSymbol) fs.GetValue<SymbolicFeatureValue>("lateral");
				if (lateralSymbol.ID == "lateral+")
				{
					switch (manner)
					{
						case ConsonantManner.Fricative:
							manner = ConsonantManner.LateralFricative;
							break;
						case ConsonantManner.Approximant:
							manner = ConsonantManner.LateralApproximant;
							break;
					}
				}
			}
			else
			{
				return false;
			}

			voiced = voiceSymbol.ID == "voice+";

			return true;
		}

		private static bool GetVowelInfo(Segment vowel, out VowelHeight height, out VowelBackness backness, out bool round)
		{
			FeatureStruct fs = vowel.FeatureStruct;
			if (vowel.IsComplex)
				fs = vowel.FeatureStruct.GetValue<FeatureStruct>(CogFeatureSystem.First);

			var heightSymbol = (FeatureSymbol) fs.GetValue<SymbolicFeatureValue>("height");
			var backnessSymbol = (FeatureSymbol) fs.GetValue<SymbolicFeatureValue>("backness");
			var roundSymbol = (FeatureSymbol) fs.GetValue<SymbolicFeatureValue>("round");

			height = VowelHeightLookup[heightSymbol.ID];
			backness = VowelBacknessLookup[backnessSymbol.ID];

			round = roundSymbol.ID == "round+";

			return true;
		}
	}
}
