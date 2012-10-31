using GraphSharp;
using GraphSharp.Algorithms.Highlight;
using SIL.Cog.ViewModels;

namespace SIL.Cog.Controls
{
	public class HierarchicalGraphHighlightAlgorithm : HighlightAlgorithmBase<HierarchicalGraphVertex, HierarchicalGraphEdge,
		IHierarchicalBidirectionalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge>, IHighlightParameters>
	{
		public HierarchicalGraphHighlightAlgorithm(IHighlightController<HierarchicalGraphVertex, HierarchicalGraphEdge, IHierarchicalBidirectionalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge>> controller,
			IHighlightParameters parameters)
			: base(controller, parameters)
		{
		}

		private void ClearSemiHighlights()
		{
			foreach ( var vertex in Controller.SemiHighlightedVertices )
				Controller.RemoveSemiHighlightFromVertex(vertex);

			foreach ( var edge in Controller.SemiHighlightedEdges )
				Controller.RemoveSemiHighlightFromEdge(edge);
		}

		private void ClearAllHighlights()
		{
			ClearSemiHighlights();

			foreach ( var vertex in Controller.HighlightedVertices )
				Controller.RemoveHighlightFromVertex(vertex);

			foreach ( var edge in Controller.HighlightedEdges )
				Controller.RemoveHighlightFromEdge(edge);
		}

		public override void ResetHighlight()
		{
			ClearAllHighlights();
		}

		public override bool OnVertexHighlighting(HierarchicalGraphVertex vertex)
		{
			ClearAllHighlights();

			if (vertex == null || !Controller.Graph.ContainsVertex(vertex))
				return false;

			//semi-highlight the out-edges
			HighlightSubtree(vertex);

			Controller.HighlightVertex(vertex, "None");
			return true;
		}

		private void HighlightSubtree(HierarchicalGraphVertex vertex)
		{
			foreach (HierarchicalGraphEdge edge in Controller.Graph.OutEdges(vertex))
			{
				Controller.SemiHighlightEdge(edge, "OutEdge");
				if (edge.Target == vertex || Controller.IsHighlightedVertex(edge.Target))
					continue;

				Controller.SemiHighlightVertex(edge.Target, "Target");
				HighlightSubtree(edge.Target);
			}
		}

		public override bool OnVertexHighlightRemoving(HierarchicalGraphVertex vertex)
		{
			ClearAllHighlights();
			return true;
		}

		public override bool OnEdgeHighlighting(HierarchicalGraphEdge edge)
		{
			ClearAllHighlights();

			//highlight the source and the target
			if (edge == null || !Controller.Graph.ContainsEdge(edge))
				return false;

			Controller.HighlightEdge(edge, null);
			Controller.SemiHighlightVertex(edge.Source, "Source");
			Controller.SemiHighlightVertex(edge.Target, "Target");
			return true;
		}

		public override bool OnEdgeHighlightRemoving(HierarchicalGraphEdge edge)
		{
			ClearAllHighlights();
			return true;
		}
	}
}
