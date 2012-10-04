using System;
using System.Globalization;
using System.Windows.Data;

namespace SIL.Cog.Views
{
	public class BooleanToYesNoConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is bool))
				return null;

			var b = (bool) value;
			return b ? "Yes" : "No";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
