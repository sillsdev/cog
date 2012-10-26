using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GraphSharp;
using SIL.Cog.Clusterers;
using SIL.Cog.Services;
using SIL.Collections;

namespace SIL.Cog.ViewModels
{
	public enum HierarchicalGraphType
	{
		Dendrogram,
		Tree
	}

	public enum ClusteringMethod
	{
		Upgma,
		NeighborJoining,
		Optics
	}

	public class HierarchicalGraphViewModel : WorkspaceViewModelBase
	{
		private CogProject _project;
		private HierarchicalGraphType _graphType;
		private ClusteringMethod _clusteringMethod;
		private IHierarchicalBidirectionalGraph<HierarchicalGraphVertex, TypedEdge<HierarchicalGraphVertex>> _graph;
		private readonly IDialogService _dialogService;
		private readonly IExportGraphService _exportGraphService;
		private SimilarityMetric _similarityMetric;

		public HierarchicalGraphViewModel(IDialogService dialogService, IExportGraphService exportGraphService)
			: base("Hierarchical Graph")
		{
			_dialogService = dialogService;
			_exportGraphService = exportGraphService;
			Messenger.Default.Register<NotificationMessage>(this, HandleNotificationMessage);

			TaskAreas.Add(new TaskAreaGroupViewModel("Graph type",
			    new CommandViewModel("Dendrogram", new RelayCommand(() => GraphType = HierarchicalGraphType.Dendrogram)),
			    new CommandViewModel("Tree", new RelayCommand(() => GraphType = HierarchicalGraphType.Tree))));
			TaskAreas.Add(new TaskAreaGroupViewModel("Clustering method",
				new CommandViewModel("UPGMA", new RelayCommand(() => ClusteringMethod = ClusteringMethod.Upgma)),
				new CommandViewModel("Neighbor-joining", new RelayCommand(() => ClusteringMethod = ClusteringMethod.NeighborJoining)),
				new CommandViewModel("Density-based", new RelayCommand(() => ClusteringMethod = ClusteringMethod.Optics))));
			TaskAreas.Add(new TaskAreaGroupViewModel("Similarity metric",
				new CommandViewModel("Lexical", new RelayCommand(() => SimilarityMetric = SimilarityMetric.Lexical)),
				new CommandViewModel("Phonetic", new RelayCommand(() => SimilarityMetric = SimilarityMetric.Phonetic))));
			TaskAreas.Add(new TaskAreaViewModel("Other tasks",
				new CommandViewModel("Export this graph", new RelayCommand(Export))));
			_graphType = HierarchicalGraphType.Dendrogram;
		}

		private void Export()
		{
			FileDialogResult result = _dialogService.ShowSaveFileDialog("Export hierarchical graph", this, new FileType("PNG image", ".png"));
			if (result.IsValid)
				_exportGraphService.ExportCurrentHierarchicalGraph(_graph, _graphType, result.FileName);
		}

		public override void Initialize(CogProject project)
		{
			_project = project;
			Graph = null;
			_project.Varieties.CollectionChanged += VarietiesChanged;
			_project.Senses.CollectionChanged += SensesChanged;
		}

		private void SensesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			Graph = null;
		}

		private void VarietiesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			Graph = null;
		}

		private void HandleNotificationMessage(NotificationMessage msg)
		{
			switch (msg.Notification)
			{
				case Notifications.ComparisonPerformed:
					GenerateGraph();
					break;
			}
		}

		private void GenerateGraph()
		{
			Graph = null;
			IEnumerable<Cluster<Variety>> clusters = null;
			switch (_clusteringMethod)
			{
				case ClusteringMethod.Upgma:
					Func<Variety, Variety, double> upgmaGetDistance = null;
					switch (_similarityMetric)
					{
						case SimilarityMetric.Lexical:
							upgmaGetDistance = (v1, v2) => 1.0 - v1.VarietyPairs[v2].LexicalSimilarityScore;
							break;
						case SimilarityMetric.Phonetic:
							upgmaGetDistance = (v1, v2) => 1.0 - v1.VarietyPairs[v2].PhoneticSimilarityScore;
							break;
					}

					var upgma = new UpgmaClusterer<Variety>(upgmaGetDistance);
					clusters = upgma.GenerateClusters(_project.Varieties);
					break;

				case ClusteringMethod.NeighborJoining:
					Func<Variety, Variety, double> njGetDistance = null;
					switch (_similarityMetric)
					{
						case SimilarityMetric.Lexical:
							njGetDistance = (v1, v2) => 1.0 - v1.VarietyPairs[v2].LexicalSimilarityScore;
							break;
						case SimilarityMetric.Phonetic:
							njGetDistance = (v1, v2) => 1.0 - v1.VarietyPairs[v2].PhoneticSimilarityScore;
							break;
					}
					var nj = new NeighborJoiningClusterer<Variety>(njGetDistance);
					clusters = nj.GenerateClusters(_project.Varieties);
					break;

				case ClusteringMethod.Optics:
					Func<Variety, IEnumerable<Tuple<Variety, double>>> getNeighbors = null;
					switch (_similarityMetric)
					{
						case SimilarityMetric.Lexical:
							getNeighbors = variety => variety.VarietyPairs.Select(pair =>
								Tuple.Create(pair.GetOtherVariety(variety), 1.0 - pair.LexicalSimilarityScore)).Concat(Tuple.Create(variety, 0.0));
							break;
						case SimilarityMetric.Phonetic:
							getNeighbors = variety => variety.VarietyPairs.Select(pair =>
								Tuple.Create(pair.GetOtherVariety(variety), 1.0 - pair.PhoneticSimilarityScore)).Concat(Tuple.Create(variety, 0.0));
							break;
					}
					var optics = new Optics<Variety>(getNeighbors, 2);
					var opticsClusterer = new OpticsDropDownClusterer<Variety>(optics);
					IList<ClusterOrderEntry<Variety>> clusterOrder = opticsClusterer.Optics.ClusterOrder(_project.Varieties);
					clusters = opticsClusterer.GenerateClusters(clusterOrder);
					break;
			}
			Debug.Assert(clusters != null);
			var graph = new HierarchicalGraph<HierarchicalGraphVertex, TypedEdge<HierarchicalGraphVertex>>();
			Cluster<Variety>[] clusterArray = clusters.ToArray();
			HierarchicalGraphVertex root;
			if (clusterArray.Length == 1)
			{
				Cluster<Variety> rootCluster = clusterArray[0];
				root = new HierarchicalGraphVertex(rootCluster.Height);
				clusters = rootCluster.Children;
			}
			else
			{
				root = new HierarchicalGraphVertex(GetMinSimilarityScore(_project.Varieties));
				clusters = clusterArray;
			}
			graph.AddVertex(root);
			foreach (Cluster<Variety> cluster in clusters)
			{
				//var vm = new HierarchicalGraphVertex(GetMinSimilarityScore(cluster.DataObjects));
				var vm = new HierarchicalGraphVertex(cluster.Height);
				graph.AddVertex(vm);
				graph.AddEdge(new TypedEdge<HierarchicalGraphVertex>(root, vm, EdgeTypes.Hierarchical));
				GenerateVertices(graph, vm, cluster);
			}
			Graph = graph;
		}

		private double GetMinSimilarityScore(IEnumerable<Variety> varieties)
		{
			double min = double.MaxValue;
			Variety[] varietyArray = varieties.ToArray();
			for (int i = 0; i < varietyArray.Length; i++)
			{
				for (int j = i + 1; j < varietyArray.Length; j++)
				{
					VarietyPair pair = varietyArray[i].VarietyPairs[varietyArray[j]];
					min = Math.Min(pair.LexicalSimilarityScore, min);
				}
			}

			return min;
		}

		private void GenerateVertices(HierarchicalGraph<HierarchicalGraphVertex, TypedEdge<HierarchicalGraphVertex>> graph, HierarchicalGraphVertex vertex, Cluster<Variety> cluster)
		{
			var childVarieties = new HashSet<Variety>();
			foreach (Cluster<Variety> child in cluster.Children)
			{
				//var vm = new HierarchicalGraphVertex(GetMinSimilarityScore(child.DataObjects));
				var vm = new HierarchicalGraphVertex(child.Height);
				graph.AddVertex(vm);
				graph.AddEdge(new TypedEdge<HierarchicalGraphVertex>(vertex, vm, EdgeTypes.Hierarchical));
				childVarieties.UnionWith(child.DataObjects);
				GenerateVertices(graph, vm, child);
			}

			foreach (Variety variety in cluster.DataObjects.Except(childVarieties))
			{
				var vm = new HierarchicalGraphVertex(variety);
				graph.AddVertex(vm);
				graph.AddEdge(new TypedEdge<HierarchicalGraphVertex>(vertex, vm, EdgeTypes.Hierarchical));
			}
		}

		public HierarchicalGraphType GraphType
		{
			get { return _graphType; }
			set { Set(() => GraphType, ref _graphType, value); }
		}

		public ClusteringMethod ClusteringMethod
		{
			get { return _clusteringMethod; }
			set
			{
				if (Set(() => ClusteringMethod, ref _clusteringMethod, value))
					GenerateGraph();
			}
		}

		public SimilarityMetric SimilarityMetric
		{
			get { return _similarityMetric; }
			set
			{
				if (Set(() => SimilarityMetric, ref _similarityMetric, value))
					GenerateGraph();
			}
		}

		public IHierarchicalBidirectionalGraph<HierarchicalGraphVertex, TypedEdge<HierarchicalGraphVertex>> Graph
		{
			get { return _graph; }
			set { Set(() => Graph, ref _graph, value); }
		}
	}
}
