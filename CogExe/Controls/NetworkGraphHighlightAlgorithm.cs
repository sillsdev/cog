using GraphSharp.Algorithms.Highlight;
using QuickGraph;
using SIL.Cog.ViewModels;

namespace SIL.Cog.Controls
{
	public class NetworkGraphHighlightAlgorithm : HighlightAlgorithmBase<NetworkGraphVertex, NetworkGraphEdge, IBidirectionalGraph<NetworkGraphVertex, NetworkGraphEdge>, NetworkGraphHighlightParameters>
	{
		public NetworkGraphHighlightAlgorithm(IHighlightController<NetworkGraphVertex, NetworkGraphEdge, IBidirectionalGraph<NetworkGraphVertex, NetworkGraphEdge>> controller,
			NetworkGraphHighlightParameters parameters)
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

		public override bool OnVertexHighlighting(NetworkGraphVertex vertex)
		{
			ClearAllHighlights();

			if (vertex == null || !Controller.Graph.ContainsVertex(vertex))
				return false;

			//semi-highlight the in-edges, and the neighbours on their other side
			foreach (NetworkGraphEdge edge in Controller.Graph.InEdges(vertex))
			{
				if (edge.SimilarityScore < Parameters.SimilarityScoreFilter)
					continue;

				Controller.SemiHighlightEdge(edge, null);
				if (edge.Source == vertex || Controller.IsHighlightedVertex(edge.Source))
					continue;

				Controller.SemiHighlightVertex(edge.Source, null);
			}

			//semi-highlight the out-edges
			foreach (NetworkGraphEdge edge in Controller.Graph.OutEdges(vertex))
			{
				if (edge.SimilarityScore < Parameters.SimilarityScoreFilter)
					continue;

				Controller.SemiHighlightEdge(edge, null);
				if (edge.Target == vertex || Controller.IsHighlightedVertex(edge.Target))
					continue;

				Controller.SemiHighlightVertex(edge.Target, null);
			}
			Controller.HighlightVertex(vertex, null);
			return true;
		}

		public override bool OnVertexHighlightRemoving(NetworkGraphVertex vertex)
		{
			ClearAllHighlights();
			return true;
		}

		public override bool OnEdgeHighlighting(NetworkGraphEdge edge)
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

		public override bool OnEdgeHighlightRemoving(NetworkGraphEdge edge)
		{
			ClearAllHighlights();
			return true;
		}
	}
}
