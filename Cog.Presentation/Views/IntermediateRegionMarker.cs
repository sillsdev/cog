using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
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
			PointLatLng latLng = selectedPointMarker != null ? selectedPointMarker.Position : Map.FromLocalToLatLng((int) point.X, (int) point.Y);

			if (_regionPoints.Count > 0)
			{
				if (latLng == Points.Last())
					return false;
				if (latLng == Position)
					return true;

				Points.Add(latLng);
				RegenerateShape(Map);
			}

			var pointMarker = new RegionPointMarker(latLng);
			_regionPoints.Add(pointMarker);
			Map.Markers.Add(pointMarker);
			return false;
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
						ctx.BeginFigure(localPath[0], false, false);

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
							Stroke = Brushes.Gray,
							StrokeThickness = 3,
							Opacity = 0.5,
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
			foreach (RegionPointMarker pm in _regionPoints)
				pm.Dispose();
			_regionPoints.Clear();

			Clear();
			Map.Markers.Remove(this);
		}
	}
}
