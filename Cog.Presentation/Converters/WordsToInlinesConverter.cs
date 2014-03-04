using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using SIL.Cog.Application.ViewModels;

namespace SIL.Cog.Presentation.Converters
{
	public class WordsToInlinesConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var words = (IEnumerable<WordViewModel>) value;

			var textDecoration = (TextDecorationCollection) parameter;

			var inlines = new List<Inline>();
			bool first = true;
			foreach (WordViewModel wordVM in words)
			{
				if (!first)
					inlines.Add(new Run(","));
				var run = new Run(wordVM.StrRep);
				if (!string.IsNullOrEmpty(((IDataErrorInfo) wordVM)["StrRep"]))
					run.TextDecorations = textDecoration;

				inlines.Add(run);
				first = false;
			}
			return inlines;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
