using System.Windows;
using GraphSharp;
using GraphSharp.Controls;
using SIL.Cog.ViewModels;

namespace SIL.Cog.Controls
{
	public class HierarchicalGraphLayout : CogGraphLayout<HierarchicalGraphVertex,
		HierarchicalGraphEdge, IHierarchicalBidirectionalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge>>
	{
		public static readonly DependencyProperty ScaleLabelsToZoomProperty = DependencyProperty.Register("ScaleLabelsToZoom", typeof(double),
			typeof(HierarchicalGraphLayout), new UIPropertyMetadata(0.0, ScaleLabelsToZoomPropertyChanged));

		private static void ScaleLabelsToZoomPropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
		{
			var graphLayout = (HierarchicalGraphLayout) depObj;
			graphLayout.ScaleLabels();
		}

		public double ScaleLabelsToZoom
		{
			get { return (double) GetValue(ScaleLabelsToZoomProperty); }
			set { SetValue(ScaleLabelsToZoomProperty, value); }
		}

		protected override void OnLayoutFinished()
		{
			base.OnLayoutFinished();
			ScaleLabels();
		}

		private void ScaleLabels()
		{
			if (Graph == null)
				return;

			double fontSize = ScaleLabelsToZoom > 1 ? 12 : (1.0 / ScaleLabelsToZoom) * 12;
			foreach (HierarchicalGraphVertex v in Graph.Vertices)
			{
				VertexControl vc = GetVertexControl(v);
				vc.FontSize = fontSize;
			}
		}
	}
}
