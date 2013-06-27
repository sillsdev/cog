using System;
using System.Globalization;
using System.Windows.Data;
using SIL.Cog.Views;

namespace SIL.Cog.Converters
{
	public class ConcurrentListConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Type type = typeof (ConcurrentList<>).MakeGenericType((Type) parameter);
			return Activator.CreateInstance(type, value);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
