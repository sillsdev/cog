using System;
using System.Globalization;
using System.Windows.Data;

namespace SIL.Cog.Converters
{
	public class StringFormatConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			var format = parameter as string;
			if (format == null)
				return null;

			return string.Format(format, values);
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
