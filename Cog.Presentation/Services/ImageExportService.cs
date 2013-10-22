using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GMap.NET.WindowsPresentation;
using GraphSharp.Algorithms.OverlapRemoval;
using GraphSharp.Controls;
using QuickGraph;
using SIL.Cog.Applications.GraphAlgorithms;
using SIL.Cog.Applications.Services;
using SIL.Cog.Applications.ViewModels;
using SIL.Cog.Presentation.Controls;

namespace SIL.Cog.Presentation.Services
{
	public class ImageExportService : IImageExportService
	{
		private readonly IDialogService _dialogService;
		private readonly IGraphService _graphService;

		public ImageExportService(IDialogService dialogService, IGraphService graphService)
		{
			_dialogService = dialogService;
			_graphService = graphService;
		}

		public bool ExportCurrentHierarchicalGraph(object ownerViewModel, HierarchicalGraphType type)
		{
			FileDialogResult result = _dialogService.ShowSaveFileDialog("Export Hierarchical Graph", ownerViewModel, new FileType("PNG image", ".png"));
			if (result.IsValid)
			{
				FrameworkElement graphLayout = null;
				switch (type)
				{
					case HierarchicalGraphType.Tree:
						graphLayout = Application.Current.MainWindow.FindVisualChild<HierarchicalGraphLayout>();
						break;
					case HierarchicalGraphType.Dendrogram:
						graphLayout = Application.Current.MainWindow.FindVisualChild<DendrogramLayout>();
						break;
				}

				if (graphLayout == null)
					throw new InvalidOperationException();

				SaveElement(graphLayout, result.FileName);
				return true;
			}
			return false;
		}

		public bool ExportHierarchicalGraph(object ownerViewModel, HierarchicalGraphType graphType, ClusteringMethod clusteringMethod, SimilarityMetric similarityMetric)
		{
			FileDialogResult result = _dialogService.ShowSaveFileDialog("Export Hierarchical Graph", ownerViewModel, new FileType("PNG image", ".png"));
			if (result.IsValid)
			{
				IBidirectionalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge> graph = _graphService.GenerateHierarchicalGraph(graphType, clusteringMethod, similarityMetric);

				FrameworkElement graphLayout = null;
				switch (graphType)
				{
					case HierarchicalGraphType.Dendrogram:
						graphLayout = new DendrogramLayout {Graph = graph, Background = Brushes.White};
						break;

					case HierarchicalGraphType.Tree:
						//double minEdgeLen = double.MaxValue, maxEdgeLen = 0;
						//foreach (HierarchicalGraphEdge edge in graph.Edges)
						//{
						//    minEdgeLen = Math.Min(minEdgeLen, edge.Length);
						//    maxEdgeLen = Math.Max(maxEdgeLen, edge.Length);
						//}
						//double x = Math.Max(0, 0.2 - (maxEdgeLen - minEdgeLen));
						//double minLen = ((40 * x) / 0.2) + 10;

						graphLayout = new HierarchicalGraphLayout
							{
								IsAnimationEnabled = false,
								CreationTransition = null,
								DestructionTransition = null,
								LayoutAlgorithmType = "RadialTree",
								//LayoutParameters = new RadialTreeLayoutParameters {BranchLengthScaling = BranchLengthScaling.FixedMinimumLength, MinimumLength = minLen},
								LayoutParameters = new RadialTreeLayoutParameters {BranchLengthScaling = BranchLengthScaling.MinimizeLabelOverlapMinimum},
								Graph = graph,
								Background = Brushes.White
							};
						graphLayout.Resources[typeof(VertexControl)] = Application.Current.Resources["HierarchicalVertexControlStyle"];
						graphLayout.Resources[typeof(EdgeControl)] = Application.Current.Resources["HierarchicalEdgeControlStyle"];
						break;
				}
				Debug.Assert(graphLayout != null);
				SaveElement(graphLayout, result.FileName);
				return true;
			}

			return false;
		}

		public bool ExportCurrentNetworkGraph(object ownerViewModel)
		{
			FileDialogResult result = _dialogService.ShowSaveFileDialog("Export Network Graph", ownerViewModel, new FileType("PNG image", ".png"));
			if (result.IsValid)
			{
				var graphLayout = Application.Current.MainWindow.FindVisualChild<NetworkGraphLayout>();
				if (graphLayout == null)
					throw new InvalidOperationException();

				SaveElement(graphLayout, result.FileName);
				return true;
			}
			return false;
		}

		public bool ExportNetworkGraph(object ownerViewModel, SimilarityMetric similarityMetric, double scoreFilter)
		{
			FileDialogResult result = _dialogService.ShowSaveFileDialog("Export Network Graph", this, new FileType("PNG image", ".png"));
			if (result.IsValid)
			{
				IBidirectionalGraph<NetworkGraphVertex, NetworkGraphEdge> graph = _graphService.GenerateNetworkGraph(similarityMetric);

				var graphLayout = new NetworkGraphLayout
					{
						IsAnimationEnabled = false,
						CreationTransition = null,
						DestructionTransition = null,
						LayoutAlgorithmType = "StressMajorization",
						LayoutParameters = new StressMajorizationLayoutParameters {WeightAdjustment = 1.0},
						OverlapRemovalAlgorithmType = "FSA",
						OverlapRemovalParameters = new OverlapRemovalParameters {HorizontalGap = 2, VerticalGap = 2},
						Graph = graph,
						Background = Brushes.White,
						WeightFilter = scoreFilter
					};
				SaveElement(graphLayout, result.FileName);
				return true;
			}

			return false;
		}

		public bool ExportCurrentMap(object ownerViewModel)
		{
			FileDialogResult result = _dialogService.ShowSaveFileDialog("Export Map", ownerViewModel, new FileType("PNG image", ".png"));
			if (result.IsValid)
			{
				var mapControl = Application.Current.MainWindow.FindVisualChild<GMapControl>();
				if (mapControl == null)
					throw new InvalidOperationException();

				var encoder = new PngBitmapEncoder();
				encoder.Frames.Add(BitmapFrame.Create((BitmapSource) mapControl.ToImageSource()));
				using (var file = File.Create(result.FileName))
					encoder.Save(file);
				return true;
			}
			return false;
		}

		public bool ExportGlobalCorrespondencesChart(object ownerViewModel, SyllablePosition syllablePosition, int frequencyFilter)
		{
			FileDialogResult result = _dialogService.ShowSaveFileDialog("Export Global Correspondences Chart", this, new FileType("PNG image", ".png"));
			if (result.IsValid)
			{
				IBidirectionalGraph<GridVertex, GlobalCorrespondenceEdge> graph = _graphService.GenerateGlobalCorrespondencesGraph(syllablePosition);

				var graphLayout = new GlobalCorrespondencesGraphLayout
					{
						IsAnimationEnabled = false,
						CreationTransition = null,
						DestructionTransition = null,
						LayoutAlgorithmType = "Grid",
						EdgeRoutingAlgorithmType = "Bundle",
						EdgeRoutingParameters = new BundleEdgeRoutingParameters {InkCoefficient = 0, LengthCoefficient = 1, VertexMargin = 2},
						Graph = graph,
						Background = Brushes.White,
						WeightFilter = frequencyFilter,
						SyllablePosition = syllablePosition
					};
				graphLayout.Resources[typeof(EdgeControl)] = Application.Current.Resources["GlobalCorrespondenceEdgeControlStyle"];
				SaveElement(graphLayout, result.FileName);
				return true;
			}

			return false;
		}

		public bool ExportCurrentGlobalCorrespondencesChart(object ownerViewModel)
		{
			FileDialogResult result = _dialogService.ShowSaveFileDialog("Export Global Correspondences Chart", ownerViewModel, new FileType("PNG image", ".png"));
			if (result.IsValid)
			{
				var graphLayout = Application.Current.MainWindow.FindVisualChild<GlobalCorrespondencesGraphLayout>();
				if (graphLayout == null)
					throw new InvalidOperationException();

				SaveElement(graphLayout, result.FileName);
				return true;
			}
			return false;
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
