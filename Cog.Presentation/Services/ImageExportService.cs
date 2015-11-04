using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GMap.NET.WindowsPresentation;
using GraphSharp.Algorithms.EdgeRouting;
using GraphSharp.Algorithms.Layout.Contextual;
using GraphSharp.Algorithms.Layout.Simple.FDP;
using GraphSharp.Algorithms.OverlapRemoval;
using GraphSharp.Controls;
using QuickGraph;
using SIL.Cog.Application.Services;
using SIL.Cog.Application.ViewModels;
using SIL.Cog.Presentation.Controls;

namespace SIL.Cog.Presentation.Services
{
	public class ImageExportService : IImageExportService
	{
		private readonly IDialogService _dialogService;
		private readonly IGraphService _graphService;
		private readonly IBusyService _busyService;

		public ImageExportService(IDialogService dialogService, IGraphService graphService, IBusyService busyService)
		{
			_dialogService = dialogService;
			_graphService = graphService;
			_busyService = busyService;
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
						graphLayout = System.Windows.Application.Current.MainWindow.FindVisualDescendants<HierarchicalGraphLayout>().FirstOrDefault();
						break;
					case HierarchicalGraphType.Dendrogram:
						graphLayout = System.Windows.Application.Current.MainWindow.FindVisualDescendants<DendrogramLayout>().FirstOrDefault();
						break;
				}

				if (graphLayout == null)
					throw new InvalidOperationException();

				SaveElement(graphLayout, result.FileName, null);
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

				Action<double> scaleUpdate = null;
				FrameworkElement graphLayout = null;
				switch (graphType)
				{
					case HierarchicalGraphType.Dendrogram:
						graphLayout = new DendrogramLayout {Graph = graph, Background = Brushes.White};
						break;

					case HierarchicalGraphType.Tree:
						var hgl = new HierarchicalGraphLayout
							{
								IsAnimationEnabled = false,
								CreationTransition = null,
								DestructionTransition = null,
								LayoutAlgorithmType = "RadialTree",
								LayoutParameters = new RadialTreeLayoutParameters {BranchLengthScaling = BranchLengthScaling.MinimizeLabelOverlapMinimum},
								Graph = graph,
								Background = Brushes.White,
								ScaleLabelsToZoom = 1.0
							};
						hgl.Resources[typeof(VertexControl)] = System.Windows.Application.Current.Resources["HierarchicalVertexControlStyle"];
						hgl.Resources[typeof(EdgeControl)] = System.Windows.Application.Current.Resources["HierarchicalEdgeControlStyle"];
						graphLayout = hgl;
						scaleUpdate = scale => hgl.ScaleLabelsToZoom = scale;
						break;
				}
				Debug.Assert(graphLayout != null);
				SaveElement(graphLayout, result.FileName, scaleUpdate);
				return true;
			}

			return false;
		}

		public bool ExportCurrentNetworkGraph(object ownerViewModel)
		{
			FileDialogResult result = _dialogService.ShowSaveFileDialog("Export Network Graph", ownerViewModel, new FileType("PNG image", ".png"));
			if (result.IsValid)
			{
				NetworkGraphLayout graphLayout = System.Windows.Application.Current.MainWindow.FindVisualDescendants<NetworkGraphLayout>().FirstOrDefault();
				if (graphLayout == null)
					throw new InvalidOperationException();

				SaveElement(graphLayout, result.FileName, null);
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
				SaveElement(graphLayout, result.FileName, null);
				return true;
			}

			return false;
		}

		public bool ExportCurrentMap(object ownerViewModel)
		{
			FileDialogResult result = _dialogService.ShowSaveFileDialog("Export Map", ownerViewModel, new FileType("PNG image", ".png"));
			if (result.IsValid)
			{
				GMapControl mapControl = System.Windows.Application.Current.MainWindow.FindVisualDescendants<GMapControl>().FirstOrDefault();
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
				IBidirectionalGraph<GlobalCorrespondencesGraphVertex, GlobalCorrespondencesGraphEdge> graph = _graphService.GenerateGlobalCorrespondencesGraph(syllablePosition);

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
				graphLayout.Resources[typeof(EdgeControl)] = System.Windows.Application.Current.Resources["GlobalCorrespondenceEdgeControlStyle"];
				SaveElement(graphLayout, result.FileName, null);
				return true;
			}

			return false;
		}

		public bool ExportCurrentGlobalCorrespondencesChart(object ownerViewModel)
		{
			FileDialogResult result = _dialogService.ShowSaveFileDialog("Export Global Correspondences Chart", ownerViewModel, new FileType("PNG image", ".png"));
			if (result.IsValid)
			{
				GlobalCorrespondencesGraphLayout graphLayout = System.Windows.Application.Current.MainWindow.FindVisualDescendants<GlobalCorrespondencesGraphLayout>().FirstOrDefault();
				if (graphLayout == null)
					throw new InvalidOperationException();

				SaveElement(graphLayout, result.FileName, null);
				return true;
			}
			return false;
		}

		private const double MaxDimension = 4800;

		private void SaveElement(FrameworkElement elem, string path, Action<double> scaleUpdate)
		{
			_busyService.ShowBusyIndicator(() =>
				{
					RenderTargetBitmap rtb;
					if (!elem.IsLoaded)
					{
						using (var temporaryPresentationSource = new HwndSource(new HwndSourceParameters()) {RootVisual = elem})
						{
							temporaryPresentationSource.Dispatcher.Invoke(DispatcherPriority.SystemIdle, new Action(() => { }));
							double width = elem.ActualWidth;
							double height = elem.ActualHeight;
							double maxDim = Math.Max(elem.ActualWidth, elem.ActualHeight);
							if (maxDim > MaxDimension)
							{
								double scale = MaxDimension / maxDim;
								if (scaleUpdate != null)
								{
									scaleUpdate(scale);
									temporaryPresentationSource.Dispatcher.Invoke(DispatcherPriority.SystemIdle, new Action(() => { }));
								}
								width = elem.ActualWidth * scale;
								height = elem.ActualHeight * scale;
								elem.RenderTransform = new ScaleTransform(scale, scale);
								temporaryPresentationSource.Dispatcher.Invoke(DispatcherPriority.SystemIdle, new Action(() => { }));
							}
							rtb = new RenderTargetBitmap((int) width, (int) height, 96, 96, PixelFormats.Pbgra32);
							rtb.Render(elem);
						}
					}
					else
					{
						double width = elem.ActualWidth;
						double height = elem.ActualHeight;
						double maxDim = Math.Max(elem.ActualWidth, elem.ActualHeight);
						Transform oldTransform = elem.RenderTransform;
						if (maxDim > MaxDimension)
						{
							double scale = MaxDimension / maxDim;
							width *= scale;
							height *= scale;
							elem.RenderTransform = new ScaleTransform(scale, scale);
						}
						elem.Measure(elem.RenderSize);
						elem.Arrange(new Rect(elem.DesiredSize));
						rtb = new RenderTargetBitmap((int) width, (int) height, 96, 96, PixelFormats.Pbgra32);
						rtb.Render(elem);
						if (maxDim > MaxDimension)
							elem.RenderTransform = oldTransform;
						elem.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
						elem.Arrange(new Rect(elem.DesiredSize));
					}

					var encoder = new PngBitmapEncoder();
					encoder.Frames.Add(BitmapFrame.Create(rtb));
					using (var file = File.Create(path))
						encoder.Save(file);
				});
		}
	}
}
