using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SIL.Cog.Presentation.Converters
{
	public class ColorBrightnessConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var color = (Color) value;
			var pcnt = (double) parameter;
			HslColor hsl = HslColor.FromColor(color);
			hsl.L += hsl.L * pcnt;
			return hsl.ToColor();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
