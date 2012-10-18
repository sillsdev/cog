using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace SIL.Cog.Converters
{
	public class ContentToPathConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			var ps = new PathSegmentCollection(4);
			var cp = (ContentPresenter)value;
			double h = cp.ActualHeight > 10 ? 1.4 * cp.ActualHeight : 10;
			double w = cp.ActualWidth > 10 ? 1.25 * cp.ActualWidth : 10;
			ps.Add(new LineSegment(new Point(1, 0.7 * h), true));
			ps.Add(new BezierSegment(new Point(1, 0.9 * h), new Point(0.1 * h, h), new Point(0.3 * h, h), true));
			ps.Add(new LineSegment(new Point(w, h), true));
			ps.Add(new BezierSegment(new Point(w + 0.6 * h, h), new Point(w + h, 0), new Point(w + h * 1.3, 0), true));

			var figure = new PathFigure(new Point(1,0), ps, false);
			var geometry = new PathGeometry();
			geometry.Figures.Add(figure);
			return geometry;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
