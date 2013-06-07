using System;
using System.Windows;
using GraphSharp;
using GraphSharp.Controls;
using QuickGraph;
using SIL.Cog.GraphAlgorithms;

namespace SIL.Cog.Controls
{
	public class CogGraphLayout<TVertex, TEdge, TGraph> : GraphLayout<TVertex, TEdge, TGraph> where TVertex : class where TEdge : IEdge<TVertex> where TGraph : class, IBidirectionalGraph<TVertex, TEdge>
	{
		static CogGraphLayout()
		{
			GraphProperty.OverrideMetadata(typeof(CogGraphLayout<TVertex, TEdge, TGraph>), new FrameworkPropertyMetadata(OnGraphPropertyChanged));
		}

		public event EventHandler LayoutFinished;

		private bool _relayoutOnVisible;

		public CogGraphLayout()
		{
			LayoutAlgorithmFactory = new CogLayoutAlgorithmFactory<TVertex, TEdge, TGraph>();
			HighlightAlgorithmFactory = new CogHighlightAlgorithmFactory<TVertex, TEdge, TGraph>();
			EdgeRoutingAlgorithmFactory = new CogEdgeRoutingAlgorithmFactory<TVertex, TEdge, TGraph>();

			IsVisibleChanged += CogGraphLayout_IsVisibleChanged;
			
			CreationTransition = new FadeTransition(0, 1, 1);
			DestructionTransition = new FadeTransition(1, 0, 1);
		}

		private static void OnGraphPropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
		{
			var gl = (CogGraphLayout<TVertex, TEdge, TGraph>) depObj;
			gl.OnGraphChanged((TGraph) e.OldValue, (TGraph) e.NewValue);
		}

		protected virtual void OnGraphChanged(TGraph oldGraph, TGraph newGraph)
		{
			if (newGraph == null)
				RemoveAllGraphElement();
			else if (!IsVisible)
				_relayoutOnVisible = true;
		} 

		protected override void OnLayoutFinished()
		{
			base.OnLayoutFinished();
			FilterByWeight();
			if (LayoutFinished != null)
				LayoutFinished(this, new EventArgs());
		}

		private void CogGraphLayout_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (IsVisible && _relayoutOnVisible)
			{
				Relayout();
				_relayoutOnVisible = false;
			}
		}

		public static readonly DependencyProperty WeightFilterProperty = DependencyProperty.Register("WeightFilter", typeof(double),
			typeof(CogGraphLayout<TVertex, TEdge, TGraph>), new UIPropertyMetadata(0.0, WeightFilterPropertyChanged));

		private static void WeightFilterPropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
		{
			var graphLayout = (CogGraphLayout<TVertex, TEdge, TGraph>) depObj;
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
				var weightedEdge = edge as WeightedEdge<TVertex>;
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
