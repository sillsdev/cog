using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SIL.Cog.Converters
{
	public class IndexToColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var index = (int) value;
			var seed = (Color) parameter;
			HslColor hsl = HslColor.FromColor(seed);
			for (int i = 0; i < index; i++)
			{
				hsl.H += 0.618033988749895;
				hsl.H %= 1;
			}

			return hsl.ToColor();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
