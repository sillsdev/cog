using System;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GraphSharp;
using QuickGraph;
using SIL.Cog.Controls;
using SIL.Cog.ViewModels;
using SIL.Cog.Views;

namespace SIL.Cog.Services
{
	public class ExportGraphService : IExportGraphService
	{
		public void ExportCurrentHierarchicalGraph(IHierarchicalBidirectionalGraph<HierarchicalGraphVertex, TypedEdge<HierarchicalGraphVertex>> graph, HierarchicalGraphType type, string path)
		{
			FrameworkElement graphLayout = null;
			switch (type)
			{
				case HierarchicalGraphType.Tree:
					graphLayout = ViewUtilities.FindVisualChild<HierarchicalGraphLayout>(Application.Current.MainWindow);
					break;
				case HierarchicalGraphType.Dendrogram:
					graphLayout = ViewUtilities.FindVisualChild<DendrogramLayout>(Application.Current.MainWindow);
					break;
			}

			if (graphLayout == null)
				throw new InvalidOperationException();

			SaveElement(graphLayout, path);

			//var graphLayout = new HierarchicalGraphLayout
			//    {
			//        IsAnimationEnabled = false,
			//        CreationTransition = null,
			//        DestructionTransition = null,
			//        LayoutAlgorithmType = "EfficientSugiyama",
			//        OverlapRemovalAlgorithmType = "FSA",
			//        Graph = graph,
			//        Background = Brushes.White
			//    };
			//SaveElement(graphLayout, path);
		}

		public void ExportCurrentNetworkGraph(IBidirectionalGraph<NetworkGraphVertex, NetworkGraphEdge> graph, string path)
		{
			var graphLayout = ViewUtilities.FindVisualChild<NetworkGraphLayout>(Application.Current.MainWindow);
			if (graphLayout == null)
				throw new InvalidOperationException();

			SaveElement(graphLayout, path);
		}

		private void SaveElement(FrameworkElement elem, string path)
		{
			RenderTargetBitmap rtb;
			if (!elem.IsLoaded)
			{
				using (var temporaryPresentationSource = new HwndSource(new HwndSourceParameters()) {RootVisual = elem})
				{
					temporaryPresentationSource.Dispatcher.Invoke(DispatcherPriority.SystemIdle, new Action(() => { }));
					rtb = new RenderTargetBitmap((int) elem.ActualWidth, (int) elem.ActualHeight, 96, 96, PixelFormats.Pbgra32);
					rtb.Render(elem);
				}
			}
			else
			{
				rtb = new RenderTargetBitmap((int) elem.ActualWidth, (int) elem.ActualHeight, 96, 96, PixelFormats.Pbgra32);
				var dv = new DrawingVisual();
				using (DrawingContext ctx = dv.RenderOpen())
				{
					var vb = new VisualBrush(elem);
					ctx.DrawRectangle(vb, null, new Rect(new Point(), new Size(elem.ActualWidth, elem.ActualHeight)));
				}
				rtb.Render(dv);
			}

			var encoder = new PngBitmapEncoder();
			encoder.Frames.Add(BitmapFrame.Create(rtb));
			using (var file = File.Create(path))
				encoder.Save(file);
		}
	}
}
