using QuickGraph;
using SIL.Cog.GraphAlgorithms;
using SIL.Cog.Views;

namespace SIL.Cog.Controls
{
	public class SimilarSegmentsGraphLayout : CogGraphLayout<GridVertex, GlobalCorrespondenceGridEdge, IBidirectionalGraph<GridVertex, GlobalCorrespondenceGridEdge>>
	{
		//private readonly List<Shape> _shapes;

		//public SimilarSegmentsGraphLayout()
		//{
		//    _shapes = new List<Shape>();
		//}

		//protected override void OnLayoutIterationFinished(IDictionary<GridVertex, Point> vertexPositions, string message)
		//{
		//    base.OnLayoutIterationFinished(vertexPositions, message);

		//    var visibilityGraph = new VisibilityGraph();
		//    IDictionary<GridVertex, Size> vertexSizes = GetLatestVertexSizes();

		//    foreach (Shape s in _shapes)
		//        Children.Remove(s);
		//    _shapes.Clear();
		//    foreach (GridVertex vertex in Graph.Vertices)
		//    {
		//        Point pos = vertexPositions[vertex];
		//        Size sz = vertexSizes[vertex];
		//        var rect = new Rect(new Point(pos.X - (sz.Width / 2), pos.Y - (sz.Height / 2)), sz);
		//        rect.Inflate(2, 2);
		//        visibilityGraph.Obstacles.Add(new Obstacle(Convert(rect.TopLeft), Convert(rect.TopRight), Convert(rect.BottomRight), Convert(rect.BottomLeft)));
		//        var rectangle = new Polyline {Stroke = new SolidColorBrush(Colors.Blue), Opacity = 0.5, };
		//        rectangle.Points.Add(rect.TopLeft);
		//        rectangle.Points.Add(rect.TopRight);
		//        rectangle.Points.Add(rect.BottomRight);
		//        rectangle.Points.Add(rect.BottomLeft);
		//        rectangle.Points.Add(rect.TopLeft);
		//        _shapes.Add(rectangle);
		//        Children.Add(rectangle);
		//    }

		//    foreach (GlobalCorrespondenceGridEdge edge in Graph.Edges)
		//    {
		//        visibilityGraph.SinglePoints.Add(Convert(vertexPositions[edge.Source]));
		//        visibilityGraph.SinglePoints.Add(Convert(vertexPositions[edge.Target]));
		//    }

		//    visibilityGraph.Compute();
		//    IUndirectedGraph<Point2D, Edge<Point2D>> graph = visibilityGraph.Graph;
		//    foreach (Edge<Point2D> edge in graph.AdjacentEdges(Convert(vertexPositions[Graph.Vertices.First(v => v.ToString() == "u")])))
		//    {
		//        var line = new Line {X1 = edge.Source.X, Y1 = edge.Source.Y, X2 = edge.Target.X, Y2 = edge.Target.Y, Stroke = new SolidColorBrush(Colors.Red), Opacity = 0.5};
		//        _shapes.Add(line);
		//        Children.Add(line);
		//    }
		//}

		//private IDictionary<GridVertex, Size> GetLatestVertexSizes()
		//{
		//    if (!IsMeasureValid)
		//        Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

		//    IDictionary<GridVertex, Size> vertexSizes =
		//        new Dictionary<GridVertex, Size>(_vertexControls.Count);

		//    //go through the vertex presenters and get the actual layoutpositions
		//    foreach (var vc in _vertexControls)
		//        vertexSizes[vc.Key] = new Size(vc.Value.ActualWidth, vc.Value.ActualHeight);

		//    return vertexSizes;
		//}

		//private static Point2D Convert(Point p)
		//{
		//    return new Point2D(p.X, p.Y);
		//}

		public override bool CanAnimate
		{
			get { return false; }
		}
	}
}
