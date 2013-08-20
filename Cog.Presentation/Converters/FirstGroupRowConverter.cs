using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SIL.Cog.Presentation.Converters
{
	public class FirstGroupRowConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values[0] == DependencyProperty.UnsetValue)
				return false;

			var groups = (ReadOnlyObservableCollection<object>) values[0];
			var itemIndex = (int) values[1];
			var groupDescCount = (int) values[2];
			if (groupDescCount > 0 && groups != null)
			{
				int index = 0;
				foreach (CollectionViewGroup group in groups)
				{
					if (index == itemIndex)
						return true;
					index += group.ItemCount;
				}
			}
			return false;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
