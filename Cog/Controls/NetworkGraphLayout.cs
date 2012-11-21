using System.Windows;
using GraphSharp.Controls;
using QuickGraph;
using SIL.Cog.ViewModels;

namespace SIL.Cog.Controls
{
	public class NetworkGraphLayout : CogGraphLayout<NetworkGraphVertex, NetworkGraphEdge, IBidirectionalGraph<NetworkGraphVertex, NetworkGraphEdge>>
	{
		public NetworkGraphLayout()
		{
			HighlightAlgorithmFactory = new NetworkGraphHighlightAlgorithmFactory();
		}

		public static readonly DependencyProperty SimilarityScoreFilterProperty = DependencyProperty.Register("SimilarityScoreFilter", typeof(double),
			typeof(NetworkGraphLayout), new UIPropertyMetadata(0.0, SimilarityScoreFilterPropertyChanged));

		private static void SimilarityScoreFilterPropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
		{
			var graphLayout = (NetworkGraphLayout) depObj;
			graphLayout.FilterBySimilarityScore();
		}

		public double SimilarityScoreFilter
		{
			get { return (double) GetValue(SimilarityScoreFilterProperty); }
			set { SetValue(SimilarityScoreFilterProperty, value); }
		}

		protected override void OnLayoutFinished()
		{
			base.OnLayoutFinished();
			FilterBySimilarityScore();
		}

		private void FilterBySimilarityScore()
		{
			if (Graph == null)
				return;

			foreach (NetworkGraphEdge edge in Graph.Edges)
			{
				EdgeControl edgeControl = GetEdgeControl(edge);
				edgeControl.Visibility = edge.SimilarityScore < SimilarityScoreFilter ? Visibility.Hidden : Visibility.Visible;
			}

			var parameters = HighlightParameters as NetworkGraphHighlightParameters;
			if (parameters != null)
				parameters.SimilarityScoreFilter = SimilarityScoreFilter;
		}
	}
}
