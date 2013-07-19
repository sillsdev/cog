using System;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using QuickGraph;
using SIL.Cog.Applications.Services;

namespace SIL.Cog.Applications.ViewModels
{
	public class NetworkGraphViewModel : WorkspaceViewModelBase
	{
		private IBidirectionalGraph<NetworkGraphVertex, NetworkGraphEdge> _graph;
		private SimilarityMetric _similarityMetric;
		private readonly IImageExportService _imageExportService;
		private readonly IProjectService _projectService;
		private readonly IGraphService _graphService;
		private double _similarityScoreFilter;

		public NetworkGraphViewModel(IProjectService projectService, IImageExportService imageExportService, IGraphService graphService)
			: base("Network Graph")
		{
			_projectService = projectService;
			_imageExportService = imageExportService;
			_graphService = graphService;

			_projectService.ProjectOpened += _projectService_ProjectOpened;

			Messenger.Default.Register<ComparisonPerformedMessage>(this, msg => Graph = _graphService.GenerateNetworkGraph(_similarityMetric));
			Messenger.Default.Register<DomainModelChangingMessage>(this, msg => Graph = null);

			TaskAreas.Add(new TaskAreaCommandGroupViewModel("Similarity metric",
				new TaskAreaCommandViewModel("Lexical", new RelayCommand(() => SimilarityMetric = SimilarityMetric.Lexical)),
				new TaskAreaCommandViewModel("Phonetic", new RelayCommand(() => SimilarityMetric = SimilarityMetric.Phonetic))));
			TaskAreas.Add(new TaskAreaItemsViewModel("Other tasks",
				new TaskAreaCommandViewModel("Export this graph", new RelayCommand(Export))));
			_similarityScoreFilter = 0.7;
		}

		private void _projectService_ProjectOpened(object sender, EventArgs e)
		{
			Graph = _projectService.Project.VarietyPairs.Count > 0 ? _graphService.GenerateNetworkGraph(_similarityMetric) : null;
		}

		private void Export()
		{
			_imageExportService.ExportCurrentNetworkGraph(this);
		}

		public SimilarityMetric SimilarityMetric
		{
			get { return _similarityMetric; }
			set
			{
				if (Set(() => SimilarityMetric, ref _similarityMetric, value) && _graph != null)
					Graph = _graphService.GenerateNetworkGraph(_similarityMetric);
			}
		}

		public double SimilarityScoreFilter
		{
			get { return _similarityScoreFilter; }
			set { Set(() => SimilarityScoreFilter, ref _similarityScoreFilter, value); }
		}

		public IBidirectionalGraph<NetworkGraphVertex, NetworkGraphEdge> Graph
		{
			get { return _graph; }
			set { Set(() => Graph, ref _graph, value); }
		}
	}
}
