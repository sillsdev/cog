using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Windows.Data;
using SIL.Cog.Views;

namespace SIL.Cog.Converters
{
	public class ConcurrentCollectionConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return new ConcurrentCollection<object>((INotifyCollectionChanged) value);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
