using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight.Messaging;
using QuickGraph;

namespace SIL.Cog.ViewModels
{
	public class NetworkGraphViewModel : WorkspaceViewModelBase
	{
		private readonly BidirectionalGraph<NetworkGraphVertex, NetworkGraphEdge> _graph;
		private CogProject _project;

		public NetworkGraphViewModel()
			: base("Network Graph")
		{
			_graph = new BidirectionalGraph<NetworkGraphVertex, NetworkGraphEdge>();
			Messenger.Default.Register<NotificationMessage>(this, HandleNotificationMessage);
		}

		public override void Initialize(CogProject project)
		{
			_project = project;
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
			_graph.Clear();
			var dict = new Dictionary<Variety, NetworkGraphVertex>();
			foreach (Variety variety in _project.Varieties)
			{
				var vertex = new NetworkGraphVertex(variety);
				_graph.AddVertex(vertex);
				dict[variety] = vertex;
			}
			foreach (VarietyPair pair in _project.VarietyPairs)
			{
				_graph.AddEdge(new NetworkGraphEdge(dict[pair.Variety1], dict[pair.Variety2], pair));
			}
		}

		public IBidirectionalGraph<NetworkGraphVertex, NetworkGraphEdge> Graph
		{
			get { return _graph; }
		}
	}
}
