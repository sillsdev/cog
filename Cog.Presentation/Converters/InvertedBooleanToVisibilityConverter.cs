using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SIL.Cog.Presentation.Converters
{
	public class InvertedBooleanToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is bool))
				return Binding.DoNothing;

			var f = (bool) value;
			return f ? Visibility.Collapsed : Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is Visibility))
				return Binding.DoNothing;

			var v = (Visibility) value;
			return v == Visibility.Collapsed;
		}
	}
}
