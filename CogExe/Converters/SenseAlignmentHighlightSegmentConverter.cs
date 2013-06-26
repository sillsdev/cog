using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using SIL.Cog.ViewModels;

namespace SIL.Cog.Converters
{
	public class SenseAlignmentHighlightSegmentConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values[2] == DependencyProperty.UnsetValue || values[3] == DependencyProperty.UnsetValue)
				return false;

			var word = (SenseAlignmentWordViewModel) values[0];
			int wordColumn = ((int) values[1]) - 1;
			var currentColumn = (int) values[2];
			var currentWord = (SenseAlignmentWordViewModel) values[3];

			return currentWord != null && currentWord != word && wordColumn == currentColumn && word.Columns[wordColumn] == currentWord.Columns[currentColumn];
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
