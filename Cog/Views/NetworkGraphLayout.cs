using GraphSharp.Controls;
using QuickGraph;
using SIL.Cog.ViewModels;

namespace SIL.Cog.Views
{
	public class NetworkGraphLayout : GraphLayout<NetworkGraphVertex, NetworkGraphEdge, IBidirectionalGraph<NetworkGraphVertex, NetworkGraphEdge>> 
	{
		public NetworkGraphLayout()
		{
			HighlightAlgorithmFactory = new NetworkGraphHighlightAlgorithmFactory();
		}
	}
}
