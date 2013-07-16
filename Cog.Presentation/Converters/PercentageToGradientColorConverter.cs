using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SIL.Cog.Presentation.Converters
{
	public class PercentageToGradientColorConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(values[0] is double) || !(values[1] is Color) || !(values[2] is Color))
				return Binding.DoNothing;

			var pcnt = (double) values[0];

			var maxColor = (Color) values[1];
			var minColor = (Color) values[2];

			int rAvg = minColor.R + (int) ((maxColor.R - minColor.R) * pcnt);
			int gAvg = minColor.G + (int) ((maxColor.G - minColor.G) * pcnt);
			int bAvg = minColor.B + (int) ((maxColor.B - minColor.B) * pcnt);

			return new SolidColorBrush(Color.FromRgb((byte) rAvg, (byte) gAvg, (byte) bAvg));
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
