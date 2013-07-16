using System;
using System.Collections.Generic;
using GraphSharp.Algorithms.Highlight;
using QuickGraph;

namespace SIL.Cog.Applications.GraphAlgorithms
{
	public class CogHighlightAlgorithmFactory<TVertex, TEdge, TGraph> : IHighlightAlgorithmFactory<TVertex, TEdge, TGraph>
		where TVertex : class
		where TEdge : IEdge<TVertex>
		where TGraph : class, IBidirectionalGraph<TVertex, TEdge>
	{
		private static readonly HashSet<string> Modes = new HashSet<string> {"Hierarchical", "Undirected"};

		public bool IsValidMode(string mode)
		{
			return Modes.Contains(mode);
		}

		public IHighlightAlgorithm<TVertex, TEdge, TGraph> CreateAlgorithm(string highlightMode, IHighlightContext<TVertex, TEdge, TGraph> context,
			IHighlightController<TVertex, TEdge, TGraph> controller, IHighlightParameters parameters)
		{
			switch (highlightMode)
			{
				case "Hierarchical":
					return new HierarchicalHighlightAlgorithm<TVertex, TEdge, TGraph>(controller, parameters);
				case "Undirected":
					return new UndirectedHighlightAlgorithm<TVertex, TEdge, TGraph>(controller, parameters as UndirectedHighlightParameters);
			}

			return null;
		}

		public IHighlightParameters CreateParameters(string highlightMode, IHighlightParameters oldParameters)
		{
			switch (highlightMode)
			{
				case "Hierarchical":
					return new HighlightParameterBase();
				case "Undirected":
					return CreateNewParameter<UndirectedHighlightParameters>(oldParameters);
			}
			return null;
		}

		public string GetHighlightMode(IHighlightAlgorithm<TVertex, TEdge, TGraph> algorithm)
		{
            if (algorithm == null)
                return string.Empty;

            int index = algorithm.GetType().Name.IndexOf("HighlightAlgorithm", StringComparison.Ordinal);
            if (index == -1)
                return string.Empty;

            string algoType = algorithm.GetType().Name;
            return algoType.Substring(0, algoType.Length - index);
		}

		public IEnumerable<string> HighlightModes
		{
			get { return Modes; }
		}

        public static TParam CreateNewParameter<TParam>(IHighlightParameters oldParameters) where TParam : class, IHighlightParameters, new()
        {
	        var parameters = oldParameters as TParam;
	        if (parameters != null)
		        return (TParam) parameters.Clone();
			return new TParam();
        }
	}
}
