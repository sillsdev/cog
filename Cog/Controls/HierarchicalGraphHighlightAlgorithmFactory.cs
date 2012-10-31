using System.Collections.Generic;
using GraphSharp;
using GraphSharp.Algorithms.Highlight;
using SIL.Cog.ViewModels;

namespace SIL.Cog.Controls
{
	public class HierarchicalGraphHighlightAlgorithmFactory : IHighlightAlgorithmFactory<HierarchicalGraphVertex, HierarchicalGraphEdge,
		IHierarchicalBidirectionalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge>>
	{
		public bool IsValidMode(string mode)
		{
			return string.IsNullOrEmpty(mode) || mode == "Simple";
		}

		public IHighlightAlgorithm<HierarchicalGraphVertex, HierarchicalGraphEdge, IHierarchicalBidirectionalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge>> CreateAlgorithm(string highlightMode,
			IHighlightContext<HierarchicalGraphVertex, HierarchicalGraphEdge, IHierarchicalBidirectionalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge>> context,
			IHighlightController<HierarchicalGraphVertex, HierarchicalGraphEdge, IHierarchicalBidirectionalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge>> controller,
			IHighlightParameters parameters)
		{
			switch (highlightMode)
			{
				case "Simple":
					return new HierarchicalGraphHighlightAlgorithm(controller, parameters);
			}

			return null;
		}

		public IHighlightParameters CreateParameters(string highlightMode, IHighlightParameters oldParameters)
		{
			return new HighlightParameterBase();
		}

		public string GetHighlightMode(IHighlightAlgorithm<HierarchicalGraphVertex, HierarchicalGraphEdge, IHierarchicalBidirectionalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge>> algorithm)
		{
			if (algorithm is HierarchicalGraphHighlightAlgorithm)
				return "Simple";
			return null;
		}

		public IEnumerable<string> HighlightModes
		{
			get { yield return "Simple"; }
		}
	}
}
