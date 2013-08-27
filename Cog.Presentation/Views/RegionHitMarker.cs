using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using GMap.NET;
using GMap.NET.WindowsPresentation;

namespace SIL.Cog.Presentation.Views
{
	public class RegionHitMarker : GMapPolygon, IDisposable
	{
		public RegionHitMarker(IList<PointLatLng> points)
			: base(points)
		{
			ZIndex = 1;
		}

		public override void RegenerateShape(GMapControl map)
		{
			if (map != null)
			{
				if (Points.Count > 1)
				{
					var localPath = new List<Point>();
					var offset = map.FromLatLngToLocal(Points[0]);
					foreach (PointLatLng i in Points)
					{
						var p = map.FromLatLngToLocal(new PointLatLng(i.Lat, i.Lng));
						localPath.Add(new Point(p.X - offset.X, p.Y - offset.Y));
					}

					// Create a StreamGeometry to use to specify myPath.
					var geometry = new StreamGeometry();

					using (StreamGeometryContext ctx = geometry.Open())
					{
						ctx.BeginFigure(localPath[0], true, true);

						// Draw a line to the next specified point.
						ctx.PolyLineTo(localPath, true, true);
					}

					// Freeze the geometry (make it unmodifiable)
					// for additional performance benefits.
					geometry.Freeze();

					// Create a path to draw a geometry with.
					Shape = new Path
						{
							Data = geometry,
							Fill = Brushes.Transparent,
							StrokeThickness = 20,
							Stroke = Brushes.Transparent,
							IsHitTestVisible = true
						};
				}
				else
				{
					Shape = null;
				}
			}
		}

		public void Dispose()
		{
			Clear();
			Map.Markers.Remove(this);
		}
	}
}
