using System.Windows;
using GraphSharp.Controls;
using QuickGraph;
using SIL.Cog.Applications.ViewModels;

namespace SIL.Cog.Presentation.Controls
{
	public class HierarchicalGraphLayout : ContextualGraphLayout<HierarchicalGraphVertex,
		HierarchicalGraphEdge, IBidirectionalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge>>
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

		    double fontSize = Scale(12);
		    var borderThickness = new Thickness(Scale(2.0));
			foreach (HierarchicalGraphVertex v in Graph.Vertices)
			{
				VertexControl vc = GetVertexControl(v);
				vc.FontSize = fontSize;
			    vc.BorderThickness = borderThickness;
			}

		    double strokeThickness = Scale(1.0);
			foreach (HierarchicalGraphEdge e in Graph.Edges)
			{
				EdgeControl ec = GetEdgeControl(e);
				ec.StrokeThickness = strokeThickness;
			}
		}

        private double Scale(double defaultValue)
        {
            return ScaleLabelsToZoom > 1 ? defaultValue : (1.0 / ScaleLabelsToZoom) * defaultValue;
        }
	}
}
