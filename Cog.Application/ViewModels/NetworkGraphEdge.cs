using System;
using GraphSharp;
using QuickGraph;
using SIL.Cog.Domain;

namespace SIL.Cog.Application.ViewModels
{
	public class NetworkGraphEdge : Edge<NetworkGraphVertex>, IWeightedEdge<NetworkGraphVertex>
	{
	    private readonly double _weight;

		public NetworkGraphEdge(NetworkGraphVertex source, NetworkGraphVertex target, VarietyPair varietyPair, SimilarityMetric similarityMetric)
			: base(source, target)
		{
			double weight = similarityMetric == SimilarityMetric.Lexical ? varietyPair.LexicalSimilarityScore : varietyPair.PhoneticSimilarityScore;
			_weight = Math.Min(weight, 0.99);
		}

	    public double Weight
	    {
	        get { return _weight; }
	    }
	}
}
