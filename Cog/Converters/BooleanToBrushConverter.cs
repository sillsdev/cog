using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SIL.Cog.Converters
{
	public class BooleanToBrushConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(values[0] is bool))
				return Binding.DoNothing;

			var trueBrush = values[1] as Brush;
			var falseBrush = values[2] as Brush;
			if (trueBrush == null || falseBrush == null)
				return Binding.DoNothing;

			var b = (bool) values[0];

			return b ? trueBrush : falseBrush;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
