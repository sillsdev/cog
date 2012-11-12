using System.Collections.Specialized;
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
			Messenger.Default.Register<NotificationMessage>(this, HandleNotificationMessage);

			TaskAreas.Add(new TaskAreaGroupViewModel("Graph type",
			    new CommandViewModel("Dendrogram", new RelayCommand(() => GraphType = HierarchicalGraphType.Dendrogram)),
			    new CommandViewModel("Tree", new RelayCommand(() => GraphType = HierarchicalGraphType.Tree))));
			TaskAreas.Add(new TaskAreaGroupViewModel("Clustering method",
				new CommandViewModel("UPGMA", new RelayCommand(() => ClusteringMethod = ClusteringMethod.Upgma)),
				new CommandViewModel("Neighbor-joining", new RelayCommand(() => ClusteringMethod = ClusteringMethod.NeighborJoining))));
			TaskAreas.Add(new TaskAreaGroupViewModel("Similarity metric",
				new CommandViewModel("Lexical", new RelayCommand(() => SimilarityMetric = SimilarityMetric.Lexical)),
				new CommandViewModel("Phonetic", new RelayCommand(() => SimilarityMetric = SimilarityMetric.Phonetic))));
			TaskAreas.Add(new TaskAreaViewModel("Other tasks",
				new CommandViewModel("Export this graph", new RelayCommand(Export))));
			_graphType = HierarchicalGraphType.Dendrogram;
		}

		private void Export()
		{
			_exportService.ExportCurrentHierarchicalGraph(this, _graphType);
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
					Graph = ViewModelUtilities.GenerateHierarchicalGraph(_project, _clusteringMethod, _similarityMetric);
					break;
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
					Graph = ViewModelUtilities.GenerateHierarchicalGraph(_project, _clusteringMethod, _similarityMetric);
			}
		}

		public SimilarityMetric SimilarityMetric
		{
			get { return _similarityMetric; }
			set
			{
				if (Set(() => SimilarityMetric, ref _similarityMetric, value))
					Graph = ViewModelUtilities.GenerateHierarchicalGraph(_project, _clusteringMethod, _similarityMetric);
			}
		}

		public IHierarchicalBidirectionalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge> Graph
		{
			get { return _graph; }
			set { Set(() => Graph, ref _graph, value); }
		}
	}
}
