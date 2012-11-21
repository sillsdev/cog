using System;
using System.Collections.Generic;
using GraphSharp;
using GraphSharp.Algorithms;
using GraphSharp.Algorithms.Layout;
using GraphSharp.Algorithms.Layout.Simple.Hierarchical;
using GraphSharp.Algorithms.Layout.Simple.Tree;
using SIL.Cog.ViewModels;
using SIL.Collections;

namespace SIL.Cog.Controls
{
	public class HierarchicalLayoutAlgorithmFactory : ILayoutAlgorithmFactory<HierarchicalGraphVertex, HierarchicalGraphEdge,
		IHierarchicalBidirectionalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge>>
	{
		public ILayoutAlgorithm<HierarchicalGraphVertex, HierarchicalGraphEdge, IHierarchicalBidirectionalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge>> CreateAlgorithm(string newAlgorithmType,
			ILayoutContext<HierarchicalGraphVertex, HierarchicalGraphEdge, IHierarchicalBidirectionalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge>> context, ILayoutParameters parameters)
		{
			if (context == null || context.Graph == null)
				return null;
			if (context.Mode == LayoutMode.Simple)
			{
				switch (newAlgorithmType)
				{
					case "RadialTree":
						return new RadialTreeLayoutAlgorithm<HierarchicalGraphVertex, HierarchicalGraphEdge, IHierarchicalBidirectionalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge>>(context.Graph,
							context.Positions, context.Sizes, parameters as RadialTreeLayoutParameters);
					case "Tree":
						return new SimpleTreeLayoutAlgorithm<HierarchicalGraphVertex, HierarchicalGraphEdge, IHierarchicalBidirectionalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge>>(context.Graph,
							context.Positions, context.Sizes, parameters as SimpleTreeLayoutParameters);
					case "EfficientSugiyama":
						return new EfficientSugiyamaLayoutAlgorithm<HierarchicalGraphVertex, HierarchicalGraphEdge, IHierarchicalBidirectionalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge>>(context.Graph,
							parameters as EfficientSugiyamaLayoutParameters, context.Sizes);
				}
			}

			return null;
		}

		public ILayoutParameters CreateParameters(string algorithmType, ILayoutParameters oldParameters)
		{
			switch (algorithmType)
			{
				case "RadialTree":
					return oldParameters.CreateNewParameter<RadialTreeLayoutParameters>();
				case "Tree":
					return oldParameters.CreateNewParameter<SimpleTreeLayoutParameters>();
				case "EfficientSugiyama":
					return oldParameters.CreateNewParameter<EfficientSugiyamaLayoutParameters>();
			}
			return null;
		}

		public bool IsValidAlgorithm(string algorithmType)
		{
			return algorithmType.IsOneOf("RadialTree", "Tree", "EfficientSugiyama");
		}

		public string GetAlgorithmType(ILayoutAlgorithm<HierarchicalGraphVertex, HierarchicalGraphEdge, IHierarchicalBidirectionalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge>> algorithm)
		{
            if (algorithm == null)
                return string.Empty;

            int index = algorithm.GetType().Name.IndexOf("LayoutAlgorithm", StringComparison.Ordinal);
            if (index == -1)
                return string.Empty;

            string algoType = algorithm.GetType().Name;
            return algoType.Substring(0, algoType.Length - index);
		}

		public bool NeedEdgeRouting(string algorithmType)
		{
			return (algorithmType != "EfficientSugiyama") && (algorithmType != "RadialTree");
		}

		public bool NeedOverlapRemoval(string algorithmType)
		{
			return false;
		}

		public IEnumerable<string> AlgorithmTypes { get; private set; }
	}
}
