using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using SIL.Cog.Presentation.Views;

namespace SIL.Cog.Presentation.Converters
{
	public class DataGridFirstGroupRowConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var row = (DataGridRow) value;
			var dataGrid = row.FindVisualAncestor<DataGrid>();
			if (dataGrid.IsGrouping)
			{
				Debug.Assert(dataGrid.Items.Groups != null);
				foreach (CollectionViewGroup group in dataGrid.Items.Groups)
				{
					if (group.ItemCount > 0 && group.Items[0].Equals(row.DataContext))
						return true;
				}
			}
			return false;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
