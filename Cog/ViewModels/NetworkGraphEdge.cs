using GraphSharp;

namespace SIL.Cog.ViewModels
{
	public class NetworkGraphEdge : WeightedEdge<NetworkGraphVertex>
	{
		private readonly VarietyPair _varietyPair;

		public NetworkGraphEdge(NetworkGraphVertex source, NetworkGraphVertex target, VarietyPair varietyPair)
			: base(source, target, varietyPair.LexicalSimilarityScore * 100.0)
		{
			_varietyPair = varietyPair;
		}

		public double LexicalSimilarityScore
		{
			get { return _varietyPair.LexicalSimilarityScore; }
		}
	}
}
