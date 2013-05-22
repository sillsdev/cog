using System;
using System.Collections.Generic;
using GraphSharp.Algorithms;
using GraphSharp.Algorithms.Layout;
using GraphSharp.Algorithms.Layout.Simple.FDP;
using GraphSharp.Algorithms.Layout.Simple.Hierarchical;
using GraphSharp.Algorithms.Layout.Simple.Tree;
using QuickGraph;

namespace SIL.Cog.GraphAlgorithms
{
	public class CogLayoutAlgorithmFactory<TVertex, TEdge, TGraph> : ILayoutAlgorithmFactory<TVertex, TEdge, TGraph>
		where TVertex : class
		where TEdge : IEdge<TVertex>
		where TGraph : class, IBidirectionalGraph<TVertex, TEdge>
	{
		private static readonly HashSet<string> Algorithms = new HashSet<string> {"StressMajorization", "LinLog", "RadialTree", "Tree", "EfficientSugiyama"};

		public ILayoutAlgorithm<TVertex, TEdge, TGraph> CreateAlgorithm(string newAlgorithmType, ILayoutContext<TVertex, TEdge, TGraph> context, ILayoutParameters parameters)
		{
			if (context == null || context.Graph == null)
				return null;
			if (context.Mode == LayoutMode.Simple)
			{
				switch (newAlgorithmType)
				{
					case "StressMajorization":
						return new StressMajorizationLayoutAlgorithm<TVertex, TEdge, TGraph>(context.Graph, context.Positions, parameters as StressMajorizationLayoutParameters);
					case "LinLog":
						return new LinLogLayoutAlgorithm<TVertex, TEdge, TGraph>(context.Graph, context.Positions, parameters as LinLogLayoutParameters);
					case "RadialTree":
						return new RadialTreeLayoutAlgorithm<TVertex, TEdge, TGraph>(context.Graph, context.Positions, context.Sizes, parameters as RadialTreeLayoutParameters);
					case "Tree":
						return new SimpleTreeLayoutAlgorithm<TVertex, TEdge, TGraph>(context.Graph, context.Positions, context.Sizes, parameters as SimpleTreeLayoutParameters);
					case "EfficientSugiyama":
						return new EfficientSugiyamaLayoutAlgorithm<TVertex, TEdge, TGraph>(context.Graph, parameters as EfficientSugiyamaLayoutParameters, context.Sizes);
				}
			}

			return null;
		}

		public ILayoutParameters CreateParameters(string algorithmType, ILayoutParameters oldParameters)
		{
			switch (algorithmType)
			{
				case "StressMajorization":
					return oldParameters.CreateNewParameter<StressMajorizationLayoutParameters>();
				case "LinLog":
					return oldParameters.CreateNewParameter<LinLogLayoutParameters>();
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
			return Algorithms.Contains(algorithmType);
		}

		public string GetAlgorithmType(ILayoutAlgorithm<TVertex, TEdge, TGraph> algorithm)
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
			return (algorithmType != "EfficientSugiyama") && (algorithmType != "RadialTree") && (algorithmType != "Tree");
		}

		public IEnumerable<string> AlgorithmTypes
		{
			get { return Algorithms; }
		}
	}
}
