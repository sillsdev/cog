using System;
using System.Globalization;
using System.Windows.Data;

namespace SIL.Cog.Presentation.Converters
{
	public class PercentageToStrokeThicknessConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(values[0] is double) || !(values[1] is int) || !(values[2] is int))
				return Binding.DoNothing;

			var pcnt = (double) values[0];

			var maxThickness = (int) values[1];
			var minThickness = (int) values[2];

			return minThickness + ((maxThickness - minThickness) * pcnt);
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
