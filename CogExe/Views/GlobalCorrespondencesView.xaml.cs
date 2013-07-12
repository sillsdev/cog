using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using GraphSharp.Controls;
using QuickGraph;
using SIL.Cog.GraphAlgorithms;
using SIL.Cog.ViewModels;

namespace SIL.Cog.Views
{
	/// <summary>
	/// Interaction logic for GlobalCorrespondencesView.xaml
	/// </summary>
	public partial class GlobalCorrespondencesView
	{
		private InputBinding _findBinding;

		public GlobalCorrespondencesView()
		{
			InitializeComponent();
			BusyCursor.DisplayUntilIdle();
		}

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var vm = DataContext as GlobalCorrespondencesViewModel;
			if (vm == null)
				return;

			vm.GlobalCorrespondences.CollectionChanged += GlobalCorrespondences_CollectionChanged;
			_findBinding = new InputBinding(vm.FindCommand, new KeyGesture(Key.F, ModifierKeys.Control));
		}

		private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var window = this.FindVisualAncestor<Window>();
			if (IsVisible)
				window.InputBindings.Add(_findBinding);
			else
				window.InputBindings.Remove(_findBinding);
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			SetGraph();
		}

		private void GlobalCorrespondences_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			SetGraph();
		}

		private void SetGraph()
		{
			var vm = (GlobalCorrespondencesViewModel) DataContext;

			if (vm.GlobalCorrespondences.Count == 0 && vm.GlobalSegments.Count == 0)
			{
				GraphLayout.Graph = null;
			}
			else
			{
				switch (vm.CorrespondenceType)
				{
					case SoundCorrespondenceType.InitialConsonants:
					case SoundCorrespondenceType.MedialConsonants:
					case SoundCorrespondenceType.FinalConsonants:
						GenerateConsonantGraph(vm);
						break;

					case SoundCorrespondenceType.Vowels:
						GenerateVowelGraph(vm);
						break;
				}
			}
		}

		private void GenerateConsonantGraph(GlobalCorrespondencesViewModel vm)
		{
			const int rowHeight = 35;
			const int columnWidth = 30;
			const int separatorWidth = 10;
			GraphLayout.LayoutParameters = new GridLayoutParameters
				{
					Rows =
						{
							// header
							new GridLayoutRow {AutoHeight = true},
							// plosive
							new GridLayoutRow {Height = rowHeight},
							// nasal
							new GridLayoutRow {Height = rowHeight},
							// trill
							new GridLayoutRow {Height = rowHeight},
							// tap or flap
							new GridLayoutRow {Height = rowHeight},
							// fricative
							new GridLayoutRow {Height = rowHeight},
							// lateral fricative
							new GridLayoutRow {Height = rowHeight},
							// approximant
							new GridLayoutRow {Height = rowHeight},
							// lateral approximant
							new GridLayoutRow {Height = rowHeight}
						},
					Columns =
						{
							// header
							new GridLayoutColumn {AutoWidth = true},
							// bilabial
							new GridLayoutColumn {Width = columnWidth},
							new GridLayoutColumn {Width = separatorWidth},
							new GridLayoutColumn {Width = columnWidth},
							// labiodental
							new GridLayoutColumn {Width = columnWidth},
							new GridLayoutColumn {Width = separatorWidth},
							new GridLayoutColumn {Width = columnWidth},
							// dental
							new GridLayoutColumn {Width = columnWidth},
							new GridLayoutColumn {Width = separatorWidth},
							new GridLayoutColumn {Width = columnWidth},
							// alveolar
							new GridLayoutColumn {Width = columnWidth},
							new GridLayoutColumn {Width = separatorWidth},
							new GridLayoutColumn {Width = columnWidth},
							// postalveolar
							new GridLayoutColumn {Width = columnWidth},
							new GridLayoutColumn {Width = separatorWidth},
							new GridLayoutColumn {Width = columnWidth},
							// retroflex
							new GridLayoutColumn {Width = columnWidth},
							new GridLayoutColumn {Width = separatorWidth},
							new GridLayoutColumn {Width = columnWidth},
							// palatal
							new GridLayoutColumn {Width = columnWidth},
							new GridLayoutColumn {Width = separatorWidth},
							new GridLayoutColumn {Width = columnWidth},
							// velar
							new GridLayoutColumn {Width = columnWidth},
							new GridLayoutColumn {Width = separatorWidth},
							new GridLayoutColumn {Width = columnWidth},
							// uvular
							new GridLayoutColumn {Width = columnWidth},
							new GridLayoutColumn {Width = separatorWidth},
							new GridLayoutColumn {Width = columnWidth},
							// pharyngeal
							new GridLayoutColumn {Width = columnWidth},
							new GridLayoutColumn {Width = separatorWidth},
							new GridLayoutColumn {Width = columnWidth},
							// glottal
							new GridLayoutColumn {Width = columnWidth},
							new GridLayoutColumn {Width = separatorWidth},
							new GridLayoutColumn {Width = columnWidth},
						}
				};

			var graph = new BidirectionalGraph<GridVertex, GlobalCorrespondenceGridEdge>();
			graph.AddVertexRange(new[]
				{
					new HeaderGridVertex("Bilabial") {Row = 0, Column = 1, ColumnSpan = 3},
					new HeaderGridVertex("Labiodental") {Row = 0, Column = 4, ColumnSpan = 3},
					new HeaderGridVertex("Dental") {Row = 0, Column = 7, ColumnSpan = 3},
					new HeaderGridVertex("Alveolar") {Row = 0, Column = 10, ColumnSpan = 3},
					new HeaderGridVertex("Postalveolar") {Row = 0, Column = 13, ColumnSpan = 3},
					new HeaderGridVertex("Retroflex") {Row = 0, Column = 16, ColumnSpan = 3},
					new HeaderGridVertex("Palatal") {Row = 0, Column = 19, ColumnSpan = 3},
					new HeaderGridVertex("Velar") {Row = 0, Column = 22, ColumnSpan = 3},
					new HeaderGridVertex("Uvular") {Row = 0, Column = 25, ColumnSpan = 3},
					new HeaderGridVertex("Pharyngeal") {Row = 0, Column = 28, ColumnSpan = 3},
					new HeaderGridVertex("Glottal") {Row = 0, Column = 31, ColumnSpan = 3},

					new HeaderGridVertex("Plosive") {Row = 1, Column = 0, HorizontalAlignment = GridHorizontalAlignment.Left},
					new HeaderGridVertex("Nasal") {Row = 2, Column = 0, HorizontalAlignment = GridHorizontalAlignment.Left},
					new HeaderGridVertex("Trill") {Row = 3, Column = 0, HorizontalAlignment = GridHorizontalAlignment.Left},
					new HeaderGridVertex("Tap or Flap") {Row = 4, Column = 0, HorizontalAlignment = GridHorizontalAlignment.Left},
					new HeaderGridVertex("Fricative") {Row = 5, Column = 0, HorizontalAlignment = GridHorizontalAlignment.Left},
					new HeaderGridVertex("Lateral fricative") {Row = 6, Column = 0, HorizontalAlignment = GridHorizontalAlignment.Left},
					new HeaderGridVertex("Approximant") {Row = 7, Column = 0, HorizontalAlignment = GridHorizontalAlignment.Left},
					new HeaderGridVertex("Lateral approximant") {Row = 8, Column = 0, HorizontalAlignment = GridHorizontalAlignment.Left}
				});

			var vertices = new Dictionary<GlobalSegmentViewModel, GlobalSegmentGridVertex>();
			foreach (ConsonantGlobalSegmentViewModel seg in vm.GlobalSegments)
			{
				var vertex = new GlobalSegmentGridVertex(seg);

				switch (seg.Manner)
				{
					case ConsonantManner.Plosive:
						vertex.Row = 1;
						break;
					case ConsonantManner.Nasal:
						vertex.Row = 2;
						break;
					case ConsonantManner.Trill:
						vertex.Row = 3;
						break;
					case ConsonantManner.TapOrFlap:
						vertex.Row = 4;
						break;
					case ConsonantManner.Fricative:
						vertex.Row = 5;
						break;
					case ConsonantManner.LateralFricative:
						vertex.Row = 6;
						break;
					case ConsonantManner.Approximant:
						vertex.Row = 7;
						break;
					case ConsonantManner.LateralApproximant:
						vertex.Row = 8;
						break;
				}

				switch (seg.Place)
				{
					case ConsonantPlace.Bilabial:
						vertex.Column = 1;
						break;
					case ConsonantPlace.Labiodental:
						vertex.Column = 4;
						break;
					case ConsonantPlace.Dental:
						vertex.Column = 7;
						break;
					case ConsonantPlace.Alveolar:
						vertex.Column = 10;
						break;
					case ConsonantPlace.Postaveolar:
						vertex.Column = 13;
						break;
					case ConsonantPlace.Retroflex:
						vertex.Column = 16;
						break;
					case ConsonantPlace.Palatal:
						vertex.Column = 19;
						break;
					case ConsonantPlace.Velar:
						vertex.Column = 22;
						break;
					case ConsonantPlace.Uvular:
						vertex.Column = 25;
						break;
					case ConsonantPlace.Pharyngeal:
						vertex.Column = 28;
						break;
					case ConsonantPlace.Glottal:
						vertex.Column = 31;
						break;
				}

				vertex.HorizontalAlignment = GridHorizontalAlignment.Right;
				if (seg.Voice)
				{
					vertex.Column += 2;
					vertex.HorizontalAlignment = GridHorizontalAlignment.Left;
				}

				vertices[seg] = vertex;
				graph.AddVertex(vertex);
			}

			graph.AddEdgeRange(vm.GlobalCorrespondences.Select(c => new GlobalCorrespondenceGridEdge(vertices[c.Segment1], vertices[c.Segment2], c)));

			GraphLayout.Graph = graph;
		}

		private void GenerateVowelGraph(GlobalCorrespondencesViewModel vm)
		{
			//if (vm.GlobalCorrespondences.Count == 0)
			//{
			//    GraphLayout.Graph = null;
			//    return;
			//}

			const int rowHeight = 35;
			const int columnWidth = 30;
			const int separatorWidth = 10;
			GraphLayout.LayoutParameters = new GridLayoutParameters
				{
					Rows =
						{
							// header
							new GridLayoutRow {AutoHeight = true},
							// close
							new GridLayoutRow {Height = rowHeight},
							// near-close
							new GridLayoutRow {Height = rowHeight},
							// close-mid
							new GridLayoutRow {Height = rowHeight},
							// mid
							new GridLayoutRow {Height = rowHeight},
							// open-mid
							new GridLayoutRow {Height = rowHeight},
							// near-open
							new GridLayoutRow {Height = rowHeight},
							// open
							new GridLayoutRow {Height = rowHeight}
						},
					Columns =
						{
							// header
							new GridLayoutColumn {AutoWidth = true},
							// front
							new GridLayoutColumn {Width = columnWidth},
							new GridLayoutColumn {Width = separatorWidth},
							new GridLayoutColumn {Width = columnWidth},
							// near-front
							new GridLayoutColumn {Width = columnWidth},
							new GridLayoutColumn {Width = separatorWidth},
							new GridLayoutColumn {Width = columnWidth},
							// central
							new GridLayoutColumn {Width = columnWidth},
							new GridLayoutColumn {Width = separatorWidth},
							new GridLayoutColumn {Width = columnWidth},
							// near-back
							new GridLayoutColumn {Width = columnWidth},
							new GridLayoutColumn {Width = separatorWidth},
							new GridLayoutColumn {Width = columnWidth},
							// back
							new GridLayoutColumn {Width = columnWidth},
							new GridLayoutColumn {Width = separatorWidth},
							new GridLayoutColumn {Width = columnWidth}
						}
				};

			var graph = new BidirectionalGraph<GridVertex, GlobalCorrespondenceGridEdge>();
			graph.AddVertex(new HeaderGridVertex("Front") {Row = 0, Column = 1, ColumnSpan = 3});
			graph.AddVertex(new HeaderGridVertex("Central") {Row = 0, Column = 7, ColumnSpan = 3});
			graph.AddVertex(new HeaderGridVertex("Back") {Row = 0, Column = 13, ColumnSpan = 3});

			graph.AddVertex(new HeaderGridVertex("Close") {Row = 1, Column = 0, HorizontalAlignment = GridHorizontalAlignment.Left});
			graph.AddVertex(new HeaderGridVertex("Close-mid") {Row = 3, Column = 0, HorizontalAlignment = GridHorizontalAlignment.Left});
			graph.AddVertex(new HeaderGridVertex("Open-mid") {Row = 5, Column = 0, HorizontalAlignment = GridHorizontalAlignment.Left});
			graph.AddVertex(new HeaderGridVertex("Open") {Row = 7, Column = 0, HorizontalAlignment = GridHorizontalAlignment.Left});

			var vertices = new Dictionary<GlobalSegmentViewModel, GlobalSegmentGridVertex>();
			foreach (VowelGlobalSegmentViewModel seg in vm.GlobalSegments)
			{
				var vertex = new GlobalSegmentGridVertex(seg);

				switch (seg.Height)
				{
					case VowelHeight.Close:
						vertex.Row = 1;
						break;
					case VowelHeight.NearClose:
						vertex.Row = 2;
						break;
					case VowelHeight.CloseMid:
						vertex.Row = 3;
						break;
					case VowelHeight.Mid:
						vertex.Row = 4;
						break;
					case VowelHeight.OpenMid:
						vertex.Row = 5;
						break;
					case VowelHeight.NearOpen:
						vertex.Row = 6;
						break;
					case VowelHeight.Open:
						vertex.Row = 7;
						break;
				}

				switch (seg.Backness)
				{
					case VowelBackness.Front:
						vertex.Column = 1;
						break;
					case VowelBackness.NearFront:
						vertex.Column = 4;
						break;
					case VowelBackness.Central:
						vertex.Column = 7;
						break;
					case VowelBackness.NearBack:
						vertex.Column = 10;
						break;
					case VowelBackness.Back:
						vertex.Column = 13;
						break;
				}

				vertex.HorizontalAlignment = GridHorizontalAlignment.Right;
				if (seg.Round)
				{
					vertex.Column += 2;
					vertex.HorizontalAlignment = GridHorizontalAlignment.Left;
				}

				vertices[seg] = vertex;
				graph.AddVertex(vertex);
			}

			graph.AddEdgeRange(vm.GlobalCorrespondences.Select(c => new GlobalCorrespondenceGridEdge(vertices[c.Segment1], vertices[c.Segment2], c)));

			GraphLayout.Graph = graph;
		}

		private void Edge_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			var vm = (GlobalCorrespondencesViewModel) DataContext;
			var edgeControl = (EdgeControl) sender;
			var corr = (GlobalCorrespondenceGridEdge) edgeControl.DataContext;
			vm.SelectedCorrespondence = vm.SelectedCorrespondence == corr.GlobalCorrespondence ? null : corr.GlobalCorrespondence;
		}
	}
}
