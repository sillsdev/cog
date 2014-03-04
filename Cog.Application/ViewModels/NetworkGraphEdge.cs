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
		    _weight = similarityMetric == SimilarityMetric.Lexical ? varietyPair.LexicalSimilarityScore : varietyPair.PhoneticSimilarityScore;
		}

	    public double Weight
	    {
	        get { return _weight; }
	    }
	}
}
