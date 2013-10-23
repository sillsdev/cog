using System;
using System.Globalization;
using System.Windows.Data;

namespace SIL.Cog.Presentation.Converters
{
	public class ProgressDialogTitleConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values[0] != null)
				return values[0];

			return string.Format("{0}% completed", values[1]);
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
