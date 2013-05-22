using GraphSharp;

namespace SIL.Cog.ViewModels
{
	public class NetworkGraphEdge : WeightedEdge<NetworkGraphVertex>
	{
		public NetworkGraphEdge(NetworkGraphVertex source, NetworkGraphVertex target, VarietyPair varietyPair, SimilarityMetric similarityMetric)
			: base(source, target, (similarityMetric == SimilarityMetric.Lexical ? varietyPair.LexicalSimilarityScore : varietyPair.PhoneticSimilarityScore))
		{
		}
	}
}
