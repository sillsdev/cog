using System;
using System.ComponentModel;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using QuickGraph;
using System.Linq;
using SIL.Cog.Application.Services;

namespace SIL.Cog.Application.ViewModels
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
		private readonly IProjectService _projectService;
		private readonly IGraphService _graphService;
		private HierarchicalGraphType _graphType;
		private ClusteringMethod _clusteringMethod;
		private IBidirectionalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge> _graph;
		private readonly IImageExportService _exportService;
		private SimilarityMetric _similarityMetric;
		private HierarchicalGraphVertex _root;

		public HierarchicalGraphViewModel(IProjectService projectService, IImageExportService exportService, IGraphService graphService)
			: base("Hierarchical Graph")
		{
			_projectService = projectService;
			_exportService = exportService;
			_graphService = graphService;

			_projectService.ProjectOpened += _projectService_ProjectOpened;

			Messenger.Default.Register<ComparisonPerformedMessage>(this, msg => Graph = _graphService.GenerateHierarchicalGraph(_graphType, _clusteringMethod, _similarityMetric));
			Messenger.Default.Register<DomainModelChangedMessage>(this, msg =>
				{
					if (msg.AffectsComparison)
						Graph = null;
				});
			Messenger.Default.Register<PerformingComparisonMessage>(this, msg => Graph = null);

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

		private void _projectService_ProjectOpened(object sender, EventArgs e)
		{
			Graph = _projectService.AreAllVarietiesCompared ? _graphService.GenerateHierarchicalGraph(_graphType, _clusteringMethod, _similarityMetric) : null;
		}

		private void Export()
		{
			if (_projectService.AreAllVarietiesCompared)
				_exportService.ExportCurrentHierarchicalGraph(this, _graphType);
		}

		public HierarchicalGraphType GraphType
		{
			get { return _graphType; }
		    set
		    {
		        if (Set(() => GraphType, ref _graphType, value) && _graph != null)
                    Graph = _graphService.GenerateHierarchicalGraph(_graphType, _clusteringMethod, _similarityMetric);
		    }
		}

		public ClusteringMethod ClusteringMethod
		{
			get { return _clusteringMethod; }
			set
			{
				if (Set(() => ClusteringMethod, ref _clusteringMethod, value) && _graph != null)
					Graph = _graphService.GenerateHierarchicalGraph(_graphType, _clusteringMethod, _similarityMetric);
			}
		}

		public SimilarityMetric SimilarityMetric
		{
			get { return _similarityMetric; }
			set
			{
				if (Set(() => SimilarityMetric, ref _similarityMetric, value) && _graph != null)
					Graph = _graphService.GenerateHierarchicalGraph(_graphType, _clusteringMethod, _similarityMetric);
			}
		}

		public IBidirectionalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge> Graph
		{
			get { return _graph; }
			private set
			{
				if (_graph != value)
				{
					Root = null;
					Set(() => Graph, ref _graph, value);
					Root = _graph == null ? null : _graph.Vertices.Single(v => _graph.IsInEdgesEmpty(v));
				}
			}
		}

		public HierarchicalGraphVertex Root
		{
			get { return _root; }
			private set { Set(() => Root, ref _root, value); }
		}
	}
}
