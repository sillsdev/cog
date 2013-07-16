using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SIL.Cog.Presentation.Converters
{
	public class PercentageToSpectrumColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is double))
				return null;

			var percentage = (double) value;

			if (percentage < 0)
				return new SolidColorBrush(Colors.White);

			double hue = (1.0 - (percentage / 100.0)) * (2.0 / 3.0);

			HslColor newColor = HslColor.FromHsl(hue, 1.0, 0.5);
			return new SolidColorBrush(newColor.ToColor());
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
