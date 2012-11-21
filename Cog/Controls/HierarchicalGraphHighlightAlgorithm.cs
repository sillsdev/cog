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

			foreach (HierarchicalGraphEdge edge in Controller.Graph.InEdges(vertex))
			{
				Controller.SemiHighlightEdge(edge, null);
				if (edge.Source == vertex || Controller.IsHighlightedVertex(edge.Source))
					continue;

				Controller.SemiHighlightVertex(edge.Source, null);
			}

			foreach (HierarchicalGraphEdge edge in Controller.Graph.OutEdges(vertex))
			{
				Controller.SemiHighlightEdge(edge, null);
				if (edge.Target == vertex || Controller.IsHighlightedVertex(edge.Target))
					continue;

				Controller.SemiHighlightVertex(edge.Target, null);
			}

			Controller.HighlightVertex(vertex, null);
			return true;
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
			Controller.SemiHighlightVertex(edge.Source, null);
			Controller.SemiHighlightVertex(edge.Target, null);
			return true;
		}

		public override bool OnEdgeHighlightRemoving(HierarchicalGraphEdge edge)
		{
			ClearAllHighlights();
			return true;
		}
	}
}
