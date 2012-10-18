using System.Collections.Generic;
using GraphSharp.Algorithms.Highlight;
using QuickGraph;
using SIL.Cog.ViewModels;

namespace SIL.Cog.Views
{
	public class NetworkGraphHighlightAlgorithmFactory : IHighlightAlgorithmFactory<NetworkGraphVertex, NetworkGraphEdge, IBidirectionalGraph<NetworkGraphVertex, NetworkGraphEdge>>
	{
		public bool IsValidMode(string mode)
		{
			return string.IsNullOrEmpty(mode) || mode == "Simple";
		}

		public IHighlightAlgorithm<NetworkGraphVertex, NetworkGraphEdge, IBidirectionalGraph<NetworkGraphVertex, NetworkGraphEdge>> CreateAlgorithm(string highlightMode,
			IHighlightContext<NetworkGraphVertex, NetworkGraphEdge, IBidirectionalGraph<NetworkGraphVertex, NetworkGraphEdge>> context,
			IHighlightController<NetworkGraphVertex, NetworkGraphEdge, IBidirectionalGraph<NetworkGraphVertex, NetworkGraphEdge>> controller,
			IHighlightParameters parameters)
		{
			switch (highlightMode)
			{
				case "Simple":
					return new NetworkGraphHighlightAlgorithm(controller, parameters);
			}

			return null;
		}

		public IHighlightParameters CreateParameters(string highlightMode, IHighlightParameters oldParameters)
		{
			return new HighlightParameterBase();
		}

		public string GetHighlightMode(IHighlightAlgorithm<NetworkGraphVertex, NetworkGraphEdge, IBidirectionalGraph<NetworkGraphVertex, NetworkGraphEdge>> algorithm)
		{
			if (algorithm is NetworkGraphHighlightAlgorithm)
				return "Simple";
			return null;
		}

		public IEnumerable<string> HighlightModes
		{
			get { yield return "Simple"; }
		}
	}
}
