using System;
using System.Collections.Generic;
using GraphSharp.Algorithms.EdgeRouting;
using GraphSharp.Algorithms.Layout;
using QuickGraph;

namespace SIL.Cog.GraphAlgorithms
{
	public class CogEdgeRoutingAlgorithmFactory<TVertex, TEdge, TGraph> : IEdgeRoutingAlgorithmFactory<TVertex, TEdge, TGraph>
		where TVertex : class
		where TEdge : IEdge<TVertex>
		where TGraph : class, IBidirectionalGraph<TVertex, TEdge>
	{
		private static readonly HashSet<string> Algorithms = new HashSet<string> {"Bundle"};

		public IEdgeRoutingAlgorithm<TVertex, TEdge, TGraph> CreateAlgorithm(string newAlgorithmType, ILayoutContext<TVertex, TEdge, TGraph> context, IEdgeRoutingParameters parameters)
		{
			switch (newAlgorithmType)
			{
				case "Bundle":
					return new BundleEdgeRoutingAlgorithm<TVertex, TEdge, TGraph>(context.Graph, context.Positions, context.Sizes, parameters as BundleEdgeRoutingParameters);
			}
			return null;
		}

		public IEdgeRoutingParameters CreateParameters(string algorithmType, IEdgeRoutingParameters oldParameters)
		{
			switch (algorithmType)
			{
				case "Bundle":
					return CreateNewParameter<BundleEdgeRoutingParameters>(oldParameters);
			}
			return null;
		}

		public bool IsValidAlgorithm(string algorithmType)
		{
			return Algorithms.Contains(algorithmType);
		}

		public string GetAlgorithmType(IEdgeRoutingAlgorithm<TVertex, TEdge, TGraph> algorithm)
		{
            if (algorithm == null)
                return string.Empty;

            int index = algorithm.GetType().Name.IndexOf("EdgeRoutingAlgorithm", StringComparison.Ordinal);
            if (index == -1)
                return string.Empty;

            string algoType = algorithm.GetType().Name;
            return algoType.Substring(0, algoType.Length - index);
		}

		public IEnumerable<string> AlgorithmTypes
		{
			get { return Algorithms; }
		}

        public static TParam CreateNewParameter<TParam>(IEdgeRoutingParameters oldParameters) where TParam : class, IEdgeRoutingParameters, new()
        {
	        var parameters = oldParameters as TParam;
	        if (parameters != null)
		        return (TParam) parameters.Clone();
			return new TParam();
        }
	}
}
