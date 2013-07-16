using GraphSharp;
using SIL.Cog.Applications.GraphAlgorithms;
using SIL.Cog.Domain;

namespace SIL.Cog.Applications.ViewModels
{
	public class NetworkGraphEdge : WeightedEdge<NetworkGraphVertex>, IWeightedEdge<NetworkGraphVertex>
	{
		public NetworkGraphEdge(NetworkGraphVertex source, NetworkGraphVertex target, VarietyPair varietyPair, SimilarityMetric similarityMetric)
			: base(source, target, (similarityMetric == SimilarityMetric.Lexical ? varietyPair.LexicalSimilarityScore : varietyPair.PhoneticSimilarityScore))
		{
		}
	}
}
