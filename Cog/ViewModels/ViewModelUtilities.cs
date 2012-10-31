using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using GraphSharp;
using QuickGraph;
using SIL.Cog.Clusterers;
using SIL.Cog.Services;
using SIL.Cog.WordListsLoaders;
using SIL.Collections;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.ViewModels
{
	public static class ViewModelUtilities
	{
		private static readonly Dictionary<FileType, IWordListsLoader> WordListsLoaders;
		static ViewModelUtilities()
		{
			WordListsLoaders = new Dictionary<FileType, IWordListsLoader>
				{
					{new FileType("Tab-delimited Text", ".txt"), new TextLoader()},
					{new FileType("WordSurv XML", ".xml"), new WordSurvXmlLoader()}
				};
		}

		public static bool ImportWordLists(IDialogService dialogService, CogProject project, object ownerViewModel)
		{
			FileDialogResult result = dialogService.ShowOpenFileDialog(ownerViewModel, "Import word lists", WordListsLoaders.Keys);
			if (result.IsValid)
			{
				project.Senses.Clear();
				project.Varieties.Clear();
				WordListsLoaders[result.SelectedFileType].Load(result.FileName, project);
				return true;
			}
			return false;
		}

		public static bool ExportSimilarityMatrix(IDialogService dialogService, CogProject project, object ownerViewModel, SimilarityMetric similarityMetric)
		{
			FileDialogResult result = dialogService.ShowSaveFileDialog("Export similarity matrix", ownerViewModel, new FileType("Tab-delimited Text", ".txt"));
			if (result.IsValid)
			{
				var optics = new Optics<Variety>(variety => variety.VarietyPairs.Select(pair =>
					{
						double score = 0;
						switch (similarityMetric)
						{
							case SimilarityMetric.Lexical:
								score = pair.LexicalSimilarityScore;
								break;
							case SimilarityMetric.Phonetic:
								score = pair.PhoneticSimilarityScore;
								break;
						}
						return Tuple.Create(pair.GetOtherVariety(variety), 1.0 - score);
					}).Concat(Tuple.Create(variety, 0.0)), 2);
				Variety[] varietyArray = optics.ClusterOrder(project.Varieties).Select(oe => oe.DataObject).ToArray();
				using (var writer = new StreamWriter(result.FileName))
				{
					foreach (Variety variety in varietyArray)
					{
						writer.Write("\t");
						writer.Write(variety.Name);
					}
					writer.WriteLine();
					for (int i = 0; i < varietyArray.Length; i++)
					{
						writer.Write(varietyArray[i].Name);
						for (int j = 0; j < varietyArray.Length; j++)
						{
							writer.Write("\t");
							if (i != j)
							{
								VarietyPair varietyPair = varietyArray[i].VarietyPairs[varietyArray[j]];
								double score = similarityMetric == SimilarityMetric.Lexical ? varietyPair.LexicalSimilarityScore : varietyPair.PhoneticSimilarityScore;
								writer.Write("{0:0.00}", score);
							}
						}
						writer.WriteLine();
					}
				}

				return true;
			}
			return false;
		}

		public static bool ExportWordLists(IDialogService dialogService, CogProject project, object ownerViewModel)
		{
			FileDialogResult result = dialogService.ShowSaveFileDialog("Export word lists", ownerViewModel, new FileType("Tab-delimited Text", ".txt"));
			if (result.IsValid)
			{
				using (var writer = new StreamWriter(result.FileName))
				{
					foreach (Sense sense in project.Senses)
					{
						writer.Write("\t");
						writer.Write(sense.Gloss);
					}
					writer.WriteLine();
					foreach (Sense sense in project.Senses)
					{
						writer.Write("\t");
						writer.Write(sense.Category);
					}
					writer.WriteLine();

					foreach (Variety variety in project.Varieties)
					{
						writer.Write(variety.Name);
						foreach (Sense sense in project.Senses)
						{
							writer.Write("\t");
							bool first = true;
							foreach (Word word in variety.Words[sense])
							{
								if (!first)
									writer.Write(",");
								writer.Write(word.StrRep);
								first = false;
							}
						}
						writer.WriteLine();
					}
				}
				return true;
			}
			return false;
		}

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
			if (clusterArray.Length == 1)
			{
				Cluster<Variety> rootCluster = clusterArray[0];
				clusterLengths = rootCluster.Children.GetLengths();
			}
			else
			{
				clusterLengths = clusterArray.Select(c => Tuple.Create(c, 0.0));
			}
			graph.AddVertex(root);
			foreach (Tuple<Cluster<Variety>, double> clusterLength in clusterLengths)
			{
				var vm = new HierarchicalGraphVertex(clusterLength.Item2);
				graph.AddVertex(vm);
				graph.AddEdge(new HierarchicalGraphEdge(root, vm, EdgeTypes.Hierarchical, clusterLength.Item2));
				GenerateVertices(graph, vm, clusterLength.Item1);
			}

			return graph;
		}

		private static void GenerateVertices(HierarchicalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge> graph, HierarchicalGraphVertex vertex, Cluster<Variety> cluster)
		{
			var childVarieties = new HashSet<Variety>();
			foreach (Tuple<Cluster<Variety>, double> child in cluster.Children.GetLengths())
			{
				HierarchicalGraphVertex newVertex;
				if (child.Item1.IsLeaf && child.Item1.DataObjects.Count == 1)
				{
					newVertex = new HierarchicalGraphVertex(child.Item1.DataObjects.First(), vertex.Depth + child.Item2);
					graph.AddVertex(newVertex);
				}
				else
				{
					newVertex = new HierarchicalGraphVertex(vertex.Depth + child.Item2);
					graph.AddVertex(newVertex);
					GenerateVertices(graph, newVertex, child.Item1);
				}
				graph.AddEdge(new HierarchicalGraphEdge(vertex, newVertex, EdgeTypes.Hierarchical, child.Item2));
				childVarieties.UnionWith(child.Item1.DataObjects);
			}

			foreach (Variety variety in cluster.DataObjects.Except(childVarieties))
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
