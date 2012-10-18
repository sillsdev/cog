using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SIL.Cog.Converters
{
	public class EnumMatchToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null || parameter == null)
				return Binding.DoNothing;
 
			string checkValue = value.ToString();
			string targetValue = parameter.ToString();
			return checkValue.Equals(targetValue, StringComparison.InvariantCultureIgnoreCase) ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
