using System.Windows;
using GraphSharp;
using GraphSharp.Algorithms.Highlight;
using GraphSharp.Controls;
using QuickGraph;

namespace SIL.Cog.Presentation.Controls
{
	public class WeightedGraphLayout<TVertex, TEdge, TGraph> : GraphLayout<TVertex, TEdge, TGraph> where TVertex : class where TEdge : IEdge<TVertex> where TGraph : class, IBidirectionalGraph<TVertex, TEdge>
	{
		protected override void OnLayoutFinished()
		{
			base.OnLayoutFinished();
			FilterByWeight();
		}

		public static readonly DependencyProperty WeightFilterProperty = DependencyProperty.Register("WeightFilter", typeof(double),
			typeof(WeightedGraphLayout<TVertex, TEdge, TGraph>), new UIPropertyMetadata(0.0, WeightFilterPropertyChanged));

		private static void WeightFilterPropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
		{
			var graphLayout = (WeightedGraphLayout<TVertex, TEdge, TGraph>) depObj;
			graphLayout.FilterByWeight();
		}

		public double WeightFilter
		{
			get { return (double) GetValue(WeightFilterProperty); }
			set { SetValue(WeightFilterProperty, value); }
		}

		private void FilterByWeight()
		{
			if (Graph == null)
				return;

			foreach (TEdge edge in Graph.Edges)
			{
				var weightedEdge = edge as IWeightedEdge<TVertex>;
				if (weightedEdge != null)
				{
					EdgeControl edgeControl = GetEdgeControl(edge);
					edgeControl.Visibility = weightedEdge.Weight < WeightFilter ? Visibility.Hidden : Visibility.Visible;
				}
			}

			var parameters = HighlightParameters as UndirectedHighlightParameters;
			if (parameters != null)
				parameters.WeightFilter = WeightFilter;
		}
	}
}
