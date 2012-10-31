using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace SIL.Cog.Converters
{
	public class EnumToFriendlyNameConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value != null)
			{
				FieldInfo fi = value.GetType().GetField(value.ToString());

				if (fi != null)
				{
					var attributes = (DescriptionAttribute[]) fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

					return ((attributes.Length > 0) && (!String.IsNullOrEmpty(attributes[0].Description)))
						? attributes[0].Description : value.ToString();
				}
			}

            return string.Empty;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
