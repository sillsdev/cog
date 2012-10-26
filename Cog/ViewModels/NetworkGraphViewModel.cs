using System.Collections.Generic;
using System.Collections.Specialized;
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
		private readonly IDialogService _dialogService;
		private readonly IExportGraphService _exportGraphService;

		public NetworkGraphViewModel(IDialogService dialogService, IExportGraphService exportGraphService)
			: base("Network Graph")
		{
			_dialogService = dialogService;
			_exportGraphService = exportGraphService;
			Messenger.Default.Register<NotificationMessage>(this, HandleNotificationMessage);

			TaskAreas.Add(new TaskAreaGroupViewModel("Similarity metric",
				new CommandViewModel("Lexical", new RelayCommand(() => SimilarityMetric = SimilarityMetric.Lexical)),
				new CommandViewModel("Phonetic", new RelayCommand(() => SimilarityMetric = SimilarityMetric.Phonetic))));
			TaskAreas.Add(new TaskAreaViewModel("Other tasks",
				new CommandViewModel("Export this graph", new RelayCommand(Export))));
		}

		private void Export()
		{
			FileDialogResult result = _dialogService.ShowSaveFileDialog("Export network graph", this, new FileType("PNG image", ".png"));
			if (result.IsValid)
				_exportGraphService.ExportCurrentNetworkGraph(_graph, result.FileName);
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
			var graph = new BidirectionalGraph<NetworkGraphVertex, NetworkGraphEdge>();
			var dict = new Dictionary<Variety, NetworkGraphVertex>();
			foreach (Variety variety in _project.Varieties)
			{
				var vertex = new NetworkGraphVertex(variety);
				graph.AddVertex(vertex);
				dict[variety] = vertex;
			}
			foreach (VarietyPair pair in _project.VarietyPairs)
				graph.AddEdge(new NetworkGraphEdge(dict[pair.Variety1], dict[pair.Variety2], pair, _similarityMetric));

			Graph = graph;
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

		public IBidirectionalGraph<NetworkGraphVertex, NetworkGraphEdge> Graph
		{
			get { return _graph; }
			set { Set(() => Graph, ref _graph, value); }
		}
	}
}
