using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SIL.Cog.Presentation.Converters
{
	public class BackgroundToForegroundConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var brush = value as SolidColorBrush;
			Color color;
			if (brush == null)
			{
				color = Colors.Black;
			}
			else
			{
				Color bgColor = brush.Color;
				// Counting the perceptive luminance - human eye favors green color... 
				double a = 1 - ( 0.299 * bgColor.R + 0.587 * bgColor.G + 0.114 * bgColor.B) / 255;
				color = a < 0.5 ? Colors.Black : Colors.White;
			}
			return new SolidColorBrush(color);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
