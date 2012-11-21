using System;
using System.Windows;
using GraphSharp.Controls;
using QuickGraph;

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
			IsVisibleChanged += CogGraphLayout_IsVisibleChanged;
			
			CreationTransition = new FadeTransition(0, 1, 1);
			DestructionTransition = new FadeTransition(1, 0, 1);
		}

		private static void OnGraphPropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
		{
			var gl = (CogGraphLayout<TVertex, TEdge, TGraph>) depObj;
			if (e.NewValue == null)
				gl.RemoveAllGraphElement();
			else if (e.NewValue != null && !gl.IsVisible)
				gl._relayoutOnVisible = true;
		}

		protected override void OnLayoutFinished()
		{
			base.OnLayoutFinished();
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
	}
}
