using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

namespace SIL.Cog.Converters
{
	public class DebugConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Debug.WriteLine(value);
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
