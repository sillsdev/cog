using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using SIL.Cog.Collections;
using SIL.Cog.Views;

namespace SIL.Cog.Converters
{
	public class ConcurrentListConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
				return null;

			Type type = typeof (ConcurrentList<>).MakeGenericType((Type) parameter);
			return Activator.CreateInstance(type, value);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
