using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SIL.Cog.Presentation.Converters
{
	public class IsTextTrimmedConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			var textBlock = (TextBlock) values[0];
			var width = (double) values[1];

			if (textBlock.TextTrimming == TextTrimming.None)
				return false;
			if (textBlock.TextWrapping != TextWrapping.NoWrap)
				return false;

			double maxWidth = textBlock.MaxWidth;
			textBlock.MaxWidth = double.PositiveInfinity;
			textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
			double totalWidth = textBlock.DesiredSize.Width;
			textBlock.MaxWidth = maxWidth;
			return (totalWidth - width) > 0.001;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
