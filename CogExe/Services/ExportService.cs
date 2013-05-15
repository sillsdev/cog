using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GMap.NET.WindowsPresentation;
using GraphSharp;
using GraphSharp.Controls;
using QuickGraph;
using SIL.Cog.Controls;
using SIL.Cog.Export;
using SIL.Cog.ViewModels;
using SIL.Cog.Views;

namespace SIL.Cog.Services
{
	public class ExportService : IExportService
	{
		private static readonly Dictionary<FileType, IWordListsExporter> WordListsExporters;
		private static readonly Dictionary<FileType, ISimilarityMatrixExporter> SimilarityMatrixExporters;
		private static readonly Dictionary<FileType, ICognateSetsExporter> CognateSetsExporters;
		private static readonly Dictionary<FileType, IVarietyPairExporter> VarietyPairExporters; 
		static ExportService()
		{
			WordListsExporters = new Dictionary<FileType, IWordListsExporter>
				{
					{new FileType("Tab-delimited Text", ".txt"), new TextWordListsExporter()},
				};

			SimilarityMatrixExporters = new Dictionary<FileType, ISimilarityMatrixExporter>
				{
					{new FileType("Tab-delimited Text", ".txt"), new TextSimilarityMatrixExporter()},
				};

			CognateSetsExporters = new Dictionary<FileType, ICognateSetsExporter>
				{
					{new FileType("Tab-delimited Text", ".txt"), new TextCognateSetsExporter()},
					{new FileType("NEXUS", ".nex"), new NexusCognateSetsExporter()}
				};
			VarietyPairExporters = new Dictionary<FileType, IVarietyPairExporter>
				{
					{new FileType("Unicode Text", ".txt"), new TextVarietyPairExporter()}
				};
		}

		private readonly IDialogService _dialogService;

		public ExportService(IDialogService dialogService)
		{
			_dialogService = dialogService;
		}

		public bool ExportSimilarityMatrix(object ownerViewModel, CogProject project, SimilarityMetric similarityMetric)
		{
			FileDialogResult result = _dialogService.ShowSaveFileDialog(ownerViewModel, "Export similarity matrix", SimilarityMatrixExporters.Keys);
			if (result.IsValid)
			{
				SimilarityMatrixExporters[result.SelectedFileType].Export(result.FileName, project, similarityMetric);
				return true;
			}
			return false;
		}

		public bool ExportWordLists(object ownerViewModel, CogProject project)
		{
			FileDialogResult result = _dialogService.ShowSaveFileDialog(ownerViewModel, "Export word lists", WordListsExporters.Keys);
			if (result.IsValid)
			{
				WordListsExporters[result.SelectedFileType].Export(result.FileName, project);
				return true;
			}
			return false;
		}

		public bool ExportCognateSets(object ownerViewModel, CogProject project)
		{
			FileDialogResult result = _dialogService.ShowSaveFileDialog(ownerViewModel, "Export cognate sets", CognateSetsExporters.Keys);
			if (result.IsValid)
			{
				CognateSetsExporters[result.SelectedFileType].Export(result.FileName, project);
				return true;
			}
			return false;
		}

		public bool ExportVarietyPair(object ownerViewModel, CogProject project, VarietyPair varietyPair)
		{
			FileDialogResult result = _dialogService.ShowSaveFileDialog(ownerViewModel, "Export variety pair", VarietyPairExporters.Keys);
			if (result.IsValid)
			{
				VarietyPairExporters[result.SelectedFileType].Export(result.FileName, project, varietyPair);
				return true;
			}
			return false;
		}

		public bool ExportCurrentHierarchicalGraph(object ownerViewModel, HierarchicalGraphType type)
		{
			FileDialogResult result = _dialogService.ShowSaveFileDialog("Export hierarchical graph", ownerViewModel, new FileType("PNG image", ".png"));
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

		public bool ExportHierarchicalGraph(object ownerViewModel, IHierarchicalBidirectionalGraph<HierarchicalGraphVertex, HierarchicalGraphEdge> graph, HierarchicalGraphType graphType)
		{
			FileDialogResult result = _dialogService.ShowSaveFileDialog("Export hierarchical graph", ownerViewModel, new FileType("PNG image", ".png"));
			if (result.IsValid)
			{
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
			FileDialogResult result = _dialogService.ShowSaveFileDialog("Export network graph", ownerViewModel, new FileType("PNG image", ".png"));
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

		public bool ExportNetworkGraph(object ownerViewModel, IBidirectionalGraph<NetworkGraphVertex, NetworkGraphEdge> graph, double scoreFilter)
		{
			FileDialogResult result = _dialogService.ShowSaveFileDialog("Export network graph", this, new FileType("PNG image", ".png"));
			if (result.IsValid)
			{
				var graphLayout = new NetworkGraphLayout
					{
						IsAnimationEnabled = false,
						CreationTransition = null,
						DestructionTransition = null,
						LayoutAlgorithmType = "LinLog",
						OverlapRemovalAlgorithmType = "FSA",
						Graph = graph,
						Background = Brushes.White,
						SimilarityScoreFilter = scoreFilter
					};
				graphLayout.Resources[typeof(EdgeControl)] = Application.Current.Resources["NetworkEdgeControlStyle"];
				graphLayout.Resources[typeof(VertexControl)] = Application.Current.Resources["NetworkVertexControlStyle"];
				SaveElement(graphLayout, result.FileName);
				return true;
			}

			return false;
		}

		public bool ExportCurrentMap(object ownerViewModel)
		{
			FileDialogResult result = _dialogService.ShowSaveFileDialog("Export map", ownerViewModel, new FileType("PNG image", ".png"));
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
