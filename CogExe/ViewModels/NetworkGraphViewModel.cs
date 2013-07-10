using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using QuickGraph;
using SIL.Cog.Services;

namespace SIL.Cog.ViewModels
{
	public class NetworkGraphViewModel : WorkspaceViewModelBase
	{
		private IBidirectionalGraph<NetworkGraphVertex, NetworkGraphEdge> _graph;
		private CogProject _project;
		private SimilarityMetric _similarityMetric;
		private readonly IExportService _exportService;
		private double _similarityScoreFilter;

		public NetworkGraphViewModel(IExportService exportService)
			: base("Network Graph")
		{
			_exportService = exportService;
			Messenger.Default.Register<Message>(this, HandleMessage);

			TaskAreas.Add(new TaskAreaCommandGroupViewModel("Similarity metric",
				new CommandViewModel("Lexical", new RelayCommand(() => SimilarityMetric = SimilarityMetric.Lexical)),
				new CommandViewModel("Phonetic", new RelayCommand(() => SimilarityMetric = SimilarityMetric.Phonetic))));
			TaskAreas.Add(new TaskAreaCommandsViewModel("Other tasks",
				new CommandViewModel("Export this graph", new RelayCommand(Export))));
			_similarityScoreFilter = 0.7;
		}

		private void Export()
		{
			_exportService.ExportCurrentNetworkGraph(this);
		}

		public override void Initialize(CogProject project)
		{
			_project = project;
			Graph = _project.VarietyPairs.Count > 0 ? _project.GenerateNetworkGraph(_similarityMetric) : null;
		}

		private void HandleMessage(Message msg)
		{
			switch (msg.Type)
			{
				case MessageType.ComparisonPerformed:
					Graph = _project.GenerateNetworkGraph(_similarityMetric);
					break;

				case MessageType.ComparisonInvalidated:
					Graph = null;
					break;
			}
		}

		public SimilarityMetric SimilarityMetric
		{
			get { return _similarityMetric; }
			set
			{
				if (Set(() => SimilarityMetric, ref _similarityMetric, value) && _graph != null)
					Graph = _project.GenerateNetworkGraph(_similarityMetric);
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
