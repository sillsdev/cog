using GraphSharp.Algorithms.Highlight;
using QuickGraph;

namespace SIL.Cog.GraphAlgorithms
{
	public class HierarchicalHighlightAlgorithm<TVertex, TEdge, TGraph> : HighlightAlgorithmBase<TVertex, TEdge, TGraph, IHighlightParameters>
		where TVertex : class
		where TEdge : IEdge<TVertex>
		where TGraph : class, IBidirectionalGraph<TVertex, TEdge>
	{
		public HierarchicalHighlightAlgorithm(IHighlightController<TVertex, TEdge, TGraph> controller, IHighlightParameters parameters)
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

		public override bool OnVertexHighlighting(TVertex vertex)
		{
			ClearAllHighlights();

			if (vertex == null || !Controller.Graph.ContainsVertex(vertex))
				return false;

			foreach (TEdge edge in Controller.Graph.InEdges(vertex))
			{
				Controller.SemiHighlightEdge(edge, null);
				if (edge.Source == vertex || Controller.IsHighlightedVertex(edge.Source))
					continue;

				Controller.SemiHighlightVertex(edge.Source, null);
			}

			foreach (TEdge edge in Controller.Graph.OutEdges(vertex))
			{
				Controller.SemiHighlightEdge(edge, null);
				if (edge.Target == vertex || Controller.IsHighlightedVertex(edge.Target))
					continue;

				Controller.SemiHighlightVertex(edge.Target, null);
			}

			Controller.HighlightVertex(vertex, null);
			return true;
		}

		public override bool OnVertexHighlightRemoving(TVertex vertex)
		{
			ClearAllHighlights();
			return true;
		}

		public override bool OnEdgeHighlighting(TEdge edge)
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

		public override bool OnEdgeHighlightRemoving(TEdge edge)
		{
			ClearAllHighlights();
			return true;
		}
	}
}
