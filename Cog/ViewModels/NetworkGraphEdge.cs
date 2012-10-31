using GraphSharp;

namespace SIL.Cog.ViewModels
{
	public class NetworkGraphEdge : WeightedEdge<NetworkGraphVertex>
	{
		private readonly VarietyPair _varietyPair;
		private readonly SimilarityMetric _similarityMetric;

		public NetworkGraphEdge(NetworkGraphVertex source, NetworkGraphVertex target, VarietyPair varietyPair, SimilarityMetric similarityMetric)
			: base(source, target, (similarityMetric == SimilarityMetric.Lexical ? varietyPair.LexicalSimilarityScore : varietyPair.PhoneticSimilarityScore) * 100.0)
		{
			_varietyPair = varietyPair;
			_similarityMetric = similarityMetric;
		}

		public double SimilarityScore
		{
			get { return _similarityMetric == SimilarityMetric.Lexical ? _varietyPair.LexicalSimilarityScore : _varietyPair.PhoneticSimilarityScore; }
		}
	}
}
