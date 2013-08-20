using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using SIL.Cog.Applications.ViewModels;

namespace SIL.Cog.Presentation.Converters
{
	public class MultipleWordAlignmentHighlightSegmentConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values[2] == DependencyProperty.UnsetValue || values[3] == DependencyProperty.UnsetValue)
				return false;

			var word = (MultipleWordAlignmentWordViewModel) values[0];
			int wordColumn = ((int) values[1]) - 2;
			var currentColumn = (int) values[2];
			var currentWord = (MultipleWordAlignmentWordViewModel) values[3];

			return currentColumn != -1 && currentWord != null && currentWord != word && wordColumn == currentColumn && word.Columns[wordColumn] == currentWord.Columns[currentColumn];
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
