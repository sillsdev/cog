using System;
using System.Globalization;
using System.Windows.Data;

namespace SIL.Cog.Converters
{
	public class BooleanToCognateStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is bool))
				return null;

			var b = (bool) value;
			return b ? "Cognates" : "Non-cognates";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
