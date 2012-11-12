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
		private readonly IExportService _exportService;

		public NetworkGraphViewModel(IExportService exportService)
			: base("Network Graph")
		{
			_exportService = exportService;
			Messenger.Default.Register<NotificationMessage>(this, HandleNotificationMessage);

			TaskAreas.Add(new TaskAreaGroupViewModel("Similarity metric",
				new CommandViewModel("Lexical", new RelayCommand(() => SimilarityMetric = SimilarityMetric.Lexical)),
				new CommandViewModel("Phonetic", new RelayCommand(() => SimilarityMetric = SimilarityMetric.Phonetic))));
			TaskAreas.Add(new TaskAreaViewModel("Other tasks",
				new CommandViewModel("Export this graph", new RelayCommand(Export))));
		}

		private void Export()
		{
			_exportService.ExportCurrentNetworkGraph(this);
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
					Graph = ViewModelUtilities.GenerateNetworkGraph(_project, _similarityMetric);
					break;
			}
		}

		public SimilarityMetric SimilarityMetric
		{
			get { return _similarityMetric; }
			set
			{
				if (Set(() => SimilarityMetric, ref _similarityMetric, value))
					Graph = ViewModelUtilities.GenerateNetworkGraph(_project, _similarityMetric);
			}
		}

		public IBidirectionalGraph<NetworkGraphVertex, NetworkGraphEdge> Graph
		{
			get { return _graph; }
			set { Set(() => Graph, ref _graph, value); }
		}
	}
}
