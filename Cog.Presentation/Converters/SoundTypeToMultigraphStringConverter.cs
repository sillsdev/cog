using System;
using System.Globalization;
using System.Windows.Data;
using SIL.Cog.Application.ViewModels;

namespace SIL.Cog.Presentation.Converters
{
	public class SoundTypeToMultigraphStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var type = (SoundType) value;
			switch (type)
			{
				case SoundType.Consonant:
					return "consonant clusters";
				case SoundType.Vowel:
					return "polyphthongs";
			}
			return Binding.DoNothing;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
