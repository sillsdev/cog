using System.Windows;
using GraphSharp.Controls;
using QuickGraph;
using SIL.Cog.GraphAlgorithms;
using SIL.Cog.ViewModels;

namespace SIL.Cog.Controls
{
	public class NetworkGraphLayout : CogGraphLayout<NetworkGraphVertex, NetworkGraphEdge, IBidirectionalGraph<NetworkGraphVertex, NetworkGraphEdge>>
	{
		public static readonly DependencyProperty WeightFilterProperty = DependencyProperty.Register("WeightFilter", typeof(double),
			typeof(NetworkGraphLayout), new UIPropertyMetadata(0.0, WeightFilterPropertyChanged));

		private static void WeightFilterPropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
		{
			var graphLayout = (NetworkGraphLayout) depObj;
			graphLayout.FilterByWeight();
		}

		public double WeightFilter
		{
			get { return (double) GetValue(WeightFilterProperty); }
			set { SetValue(WeightFilterProperty, value); }
		}

		protected override void OnLayoutFinished()
		{
			base.OnLayoutFinished();
			FilterByWeight();
		}

		private void FilterByWeight()
		{
			if (Graph == null)
				return;

			foreach (NetworkGraphEdge edge in Graph.Edges)
			{
				EdgeControl edgeControl = GetEdgeControl(edge);
				edgeControl.Visibility = edge.Weight < WeightFilter ? Visibility.Hidden : Visibility.Visible;
			}

			var parameters = HighlightParameters as UndirectedHighlightParameters;
			if (parameters != null)
				parameters.WeightFilter = WeightFilter;
		}
	}
}
