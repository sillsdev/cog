using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using GMap.NET;
using GMap.NET.WindowsPresentation;

namespace SIL.Cog.Views
{
	public class RegionPointMarker : GMapMarker, IDisposable
	{
		private bool _isMidpoint;

		public RegionPointMarker(PointLatLng pos)
			: base(pos)
		{
			var rect = new Rectangle
				{
					Stroke = Brushes.Black,
					Fill = Brushes.White,
					Width = 8,
					Height = 8,
					VerticalAlignment = VerticalAlignment.Center,
					HorizontalAlignment = HorizontalAlignment.Center
				};
			Shape = rect;
			Offset = new Point(-4, -4);
			ZIndex = 10;
			Shape.MouseEnter += Shape_MouseEnter;
			Shape.MouseLeave += Shape_MouseLeave;
		}

		private void Shape_MouseEnter(object sender, MouseEventArgs e)
		{
			var rect = (Rectangle) sender;
			rect.Fill = Brushes.LightGray;
		}

		private void Shape_MouseLeave(object sender, MouseEventArgs e)
		{
			var rect = (Rectangle) sender;
			rect.Fill = Brushes.White;
		}

		public bool IsMidpoint
		{
			get { return _isMidpoint; }
			set
			{
				_isMidpoint = value;
				Shape.Opacity = _isMidpoint ? 0.5 : 1.0;
			}
		}

		public void Dispose()
		{
			Clear();
			Map.Markers.Remove(this);
		}
	}
}
