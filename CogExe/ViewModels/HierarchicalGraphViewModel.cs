using System.ComponentModel;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GraphSharp;
using SIL.Cog.Services;

namespace SIL.Cog.ViewModels
{
	public enum HierarchicalGraphType
	{
		[Description("Dendrogram")]
		Dendrogram,
		[Description("Tree")]
		Tree
	}

	public enum ClusteringMethod
	{
		[Description("UPGMA")]
		Upgma,
		[Description("Neighbor-joining")]
		NeighborJoining
	}

	public class HierarchicalGraphViewModel : WorkspaceViewModelBase
	{
		private CogProject _project;
		private HierarchicalGraphType _graphType;
		private ClusteringMethod _clusteringMethod;
		private IHierarchicalBidirectionalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge> _graph;
		private readonly IExportService _exportService;
		private SimilarityMetric _similarityMetric;

		public HierarchicalGraphViewModel(IExportService exportService)
			: base("Hierarchical Graph")
		{
			_exportService = exportService;

			Messenger.Default.Register<ComparisonPerformedMessage>(this, msg => Graph = _project.GenerateHierarchicalGraph(_graphType, _clusteringMethod, _similarityMetric));
			Messenger.Default.Register<ModelChangingMessage>(this, msg => Graph = null);

			TaskAreas.Add(new TaskAreaCommandGroupViewModel("Graph type",
			    new TaskAreaCommandViewModel("Dendrogram", new RelayCommand(() => GraphType = HierarchicalGraphType.Dendrogram)),
			    new TaskAreaCommandViewModel("Tree", new RelayCommand(() => GraphType = HierarchicalGraphType.Tree))));
			TaskAreas.Add(new TaskAreaCommandGroupViewModel("Clustering method",
				new TaskAreaCommandViewModel("UPGMA", new RelayCommand(() => ClusteringMethod = ClusteringMethod.Upgma)),
				new TaskAreaCommandViewModel("Neighbor-joining", new RelayCommand(() => ClusteringMethod = ClusteringMethod.NeighborJoining))));
			TaskAreas.Add(new TaskAreaCommandGroupViewModel("Similarity metric",
				new TaskAreaCommandViewModel("Lexical", new RelayCommand(() => SimilarityMetric = SimilarityMetric.Lexical)),
				new TaskAreaCommandViewModel("Phonetic", new RelayCommand(() => SimilarityMetric = SimilarityMetric.Phonetic))));
			TaskAreas.Add(new TaskAreaItemsViewModel("Other tasks",
				new TaskAreaCommandViewModel("Export this graph", new RelayCommand(Export))));
			_graphType = HierarchicalGraphType.Dendrogram;
		}

		private void Export()
		{
			_exportService.ExportCurrentHierarchicalGraph(this, _graphType);
		}

		public override void Initialize(CogProject project)
		{
			_project = project;
			Graph = _project.VarietyPairs.Count > 0 ? _project.GenerateHierarchicalGraph(_graphType, _clusteringMethod, _similarityMetric) : null;
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
				if (Set(() => ClusteringMethod, ref _clusteringMethod, value) && _graph != null)
					Graph = _project.GenerateHierarchicalGraph(_graphType, _clusteringMethod, _similarityMetric);
			}
		}

		public SimilarityMetric SimilarityMetric
		{
			get { return _similarityMetric; }
			set
			{
				if (Set(() => SimilarityMetric, ref _similarityMetric, value) && _graph != null)
					Graph = _project.GenerateHierarchicalGraph(_graphType, _clusteringMethod, _similarityMetric);
			}
		}

		public IHierarchicalBidirectionalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge> Graph
		{
			get { return _graph; }
			set { Set(() => Graph, ref _graph, value); }
		}
	}
}
