using System;
using System.Globalization;
using System.Windows.Data;

namespace SIL.Cog.Converters
{
	public class EqualityConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values == null || values.Length == 0) return false;

			for (int i = 1; i < values.Length; i++)
			{
				if (values[0] == null && values[i] != null) return false;
				if (values[i] == null || !values[i].Equals(values[0])) return false;
			}

			return true;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
