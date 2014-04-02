using System;
using System.Globalization;
using System.Windows.Data;

namespace SIL.Cog.Presentation.Converters
{
	public class CognateSetNameConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var index = (int) value;
			if (index == int.MaxValue)
				return "Non-cognates";
			return string.Format("Cognate set {0}", index);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
