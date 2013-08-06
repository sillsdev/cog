using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace SIL.Cog.Presentation.Behaviors
{
	public class InsertionAdorner : Adorner
	{
		private readonly bool _isSeparatorHorizontal;
		public bool IsInFirstHalf { get; set; }
		private readonly AdornerLayer _adornerLayer;
		private static readonly Pen Pen;
		private static readonly PathGeometry Triangle;

		// Create the pen and triangle in a static constructor and freeze them to improve performance.
		static InsertionAdorner()
		{
			Pen = new Pen { Brush = Brushes.Gray, Thickness = 2 };
			Pen.Freeze();

			var firstLine = new LineSegment(new Point(0, -5), false);
			firstLine.Freeze();
			var secondLine = new LineSegment(new Point(0, 5), false);
			secondLine.Freeze();

			var figure = new PathFigure { StartPoint = new Point(5, 0) };
			figure.Segments.Add(firstLine);
			figure.Segments.Add(secondLine);
			figure.Freeze();

			Triangle = new PathGeometry();
			Triangle.Figures.Add(figure);
			Triangle.Freeze();
		}

		public InsertionAdorner(bool isSeparatorHorizontal, bool isInFirstHalf, UIElement adornedElement, AdornerLayer adornerLayer)
			: base(adornedElement)
		{
			_isSeparatorHorizontal = isSeparatorHorizontal;
			IsInFirstHalf = isInFirstHalf;
			_adornerLayer = adornerLayer;
			IsHitTestVisible = false;

			_adornerLayer.Add(this);
		}

		// This draws one line and two triangles at each end of the line.
		protected override void OnRender(DrawingContext drawingContext)
		{
			Point startPoint;
			Point endPoint;

			CalculateStartAndEndPoint(out startPoint, out endPoint);
			drawingContext.DrawLine(Pen, startPoint, endPoint);

			if (_isSeparatorHorizontal)
			{
				DrawTriangle(drawingContext, startPoint, 0);
				DrawTriangle(drawingContext, endPoint, 180);
			}
			else
			{
				DrawTriangle(drawingContext, startPoint, 90);
				DrawTriangle(drawingContext, endPoint, -90);
			}
		}

		private void DrawTriangle(DrawingContext drawingContext, Point origin, double angle)
		{
			drawingContext.PushTransform(new TranslateTransform(origin.X, origin.Y));
			drawingContext.PushTransform(new RotateTransform(angle));

			drawingContext.DrawGeometry(Pen.Brush, null, Triangle);

			drawingContext.Pop();
			drawingContext.Pop();
		}

		private void CalculateStartAndEndPoint(out Point startPoint, out Point endPoint)
		{
			startPoint = new Point();
			endPoint = new Point();
			
			double width = AdornedElement.RenderSize.Width;
			double height = AdornedElement.RenderSize.Height;

			if (_isSeparatorHorizontal)
			{
				endPoint.X = width;
				if (!IsInFirstHalf)
				{
					startPoint.Y = height;
					endPoint.Y = height;
				}
			}
			else
			{
				endPoint.Y = height;
				if (!IsInFirstHalf)
				{
					startPoint.X = width;
					endPoint.X = width;
				}
			}		
		}

		public void Detach()
		{
			_adornerLayer.Remove(this);
		}

	}
}
