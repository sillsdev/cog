using GraphSharp;

namespace SIL.Cog.ViewModels
{
	public class NetworkGraphEdge : WeightedEdge<NetworkGraphVertex>
	{
		private readonly VarietyPair _varietyPair;

		public NetworkGraphEdge(NetworkGraphVertex source, NetworkGraphVertex target, VarietyPair varietyPair, SimilarityMetric similarityMetric)
			: base(source, target, (similarityMetric == SimilarityMetric.Lexical ? varietyPair.LexicalSimilarityScore : varietyPair.PhoneticSimilarityScore) * 100.0)
		{
			_varietyPair = varietyPair;
		}

		public double LexicalSimilarityScore
		{
			get { return _varietyPair.LexicalSimilarityScore; }
		}
	}
}
