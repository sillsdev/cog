using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using SIL.Collections;

namespace SIL.Cog.Presentation.Views
{
	public class IntermediateRegionMarker : GMapRoute, IDisposable
	{
		private readonly List<RegionPointMarker> _regionPoints;

		public IntermediateRegionMarker(PointLatLng pos)
			: base(pos.ToEnumerable())
		{
			_regionPoints = new List<RegionPointMarker>();
		}

		public bool AddPoint(Point point)
		{
			GMapMarker selectedPointMarker = _regionPoints.FirstOrDefault(p => p.Shape.IsMouseOver);
			PointLatLng latLng = selectedPointMarker?.Position ?? Map.FromLocalToLatLng((int) point.X, (int) point.Y);

			if (_regionPoints.Count > 0)
			{
				if (latLng == Points.Last())
					return false;
				if (latLng == Position)
					return true;

				Points.Add(latLng);
				Map.RegenerateShape(this);
			}

			var pointMarker = new RegionPointMarker(latLng);
			_regionPoints.Add(pointMarker);
			Map.Markers.Add(pointMarker);
			return false;
		}

		public override Path CreatePath(List<Point> localPath, bool addBlurEffect)
		{
			// Create a StreamGeometry to use to specify myPath.
			var geometry = new StreamGeometry();

			using (StreamGeometryContext ctx = geometry.Open())
			{
				ctx.BeginFigure(localPath[0], false, false);

				// Draw a line to the next specified point.
				ctx.PolyLineTo(localPath, true, true);
			}

			// Freeze the geometry (make it unmodifiable)
			// for additional performance benefits.
			geometry.Freeze();

			// Create a path to draw a geometry with.
			var myPath = new Path();
			{
				// Specify the shape of the Path using the StreamGeometry.
				myPath.Data = geometry;

				if (addBlurEffect)
				{
					var ef = new BlurEffect();
					{
						ef.KernelType = KernelType.Gaussian;
						ef.Radius = 3.0;
						ef.RenderingBias = RenderingBias.Performance;
					}

					myPath.Effect = ef;
				}

				myPath.Stroke = Brushes.Gray;
				myPath.StrokeThickness = 3;
				myPath.StrokeLineJoin = PenLineJoin.Round;
				myPath.StrokeStartLineCap = PenLineCap.Triangle;
				myPath.StrokeEndLineCap = PenLineCap.Square;

				myPath.Opacity = 0.5;
				myPath.IsHitTestVisible = true;
			}
			return myPath;
		}

		public void Dispose()
		{
			foreach (RegionPointMarker pm in _regionPoints)
				pm.Dispose();
			_regionPoints.Clear();

			Clear();
			Map.Markers.Remove(this);
		}
	}
}
