using System;
using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight.Messaging;
using GraphSharp;
using SIL.Cog.Clusterers;
using SIL.Collections;

namespace SIL.Cog.ViewModels
{
	public class HierarchicalGraphViewModel : WorkspaceViewModelBase
	{
		private CogProject _project;
		private readonly HierarchicalGraph<HierarchicalGraphVertex, TypedEdge<HierarchicalGraphVertex>> _graph;

		public HierarchicalGraphViewModel()
			: base("Hierarchical Graph")
		{
			_graph = new HierarchicalGraph<HierarchicalGraphVertex, TypedEdge<HierarchicalGraphVertex>>();
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
			var optics = new Optics<Variety>(variety => variety.VarietyPairs.Select(pair =>
				Tuple.Create(variety == pair.Variety1 ? pair.Variety2 : pair.Variety1, 1.0 - pair.LexicalSimilarityScore)).Concat(Tuple.Create(variety, 0.0)), 2);
			var opticsClusterer = new OpticsDropDownClusterer<Variety>(optics);
			IList<ClusterOrderEntry<Variety>> clusterOrder = opticsClusterer.Optics.ClusterOrder(_project.Varieties);
			IEnumerable<Cluster<Variety>> clusters = opticsClusterer.GenerateClusters(clusterOrder);
			_graph.Clear();
			var root = new HierarchicalGraphVertex();
			_graph.AddVertex(root);
			foreach (Cluster<Variety> cluster in clusters)
			{
				var vm = new HierarchicalGraphVertex();
				_graph.AddVertex(vm);
				_graph.AddEdge(new TypedEdge<HierarchicalGraphVertex>(root, vm, EdgeTypes.Hierarchical));
				GenerateVertices(vm, cluster);
			}
		}

		private void GenerateVertices(HierarchicalGraphVertex vertex, Cluster<Variety> cluster)
		{
			var childVarieties = new HashSet<Variety>();
			foreach (Cluster<Variety> child in cluster.Children)
			{
				var vm = new HierarchicalGraphVertex();
				_graph.AddVertex(vm);
				_graph.AddEdge(new TypedEdge<HierarchicalGraphVertex>(vertex, vm, EdgeTypes.Hierarchical));
				childVarieties.UnionWith(child.DataObjects);
				GenerateVertices(vm, child);
			}

			foreach (Variety variety in cluster.DataObjects.Except(childVarieties))
			{
				var vm = new HierarchicalGraphVertex(variety);
				_graph.AddVertex(vm);
				_graph.AddEdge(new TypedEdge<HierarchicalGraphVertex>(vertex, vm, EdgeTypes.Hierarchical));
			}
		}

		public IHierarchicalBidirectionalGraph<HierarchicalGraphVertex, TypedEdge<HierarchicalGraphVertex>> Graph
		{
			get { return _graph; }
		}
	}
}
