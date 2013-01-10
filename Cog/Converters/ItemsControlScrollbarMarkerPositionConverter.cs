using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace SIL.Cog.Converters
{
	public class ItemsControlScrollbarMarkerPositionConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			object item = values[0];
			var itemsControl = (ItemsControl) values[1];
			var sv = (ScrollViewer) values[2];

			var cp = (ContentPresenter) itemsControl.ItemContainerGenerator.ContainerFromItem(item);
			if (cp == null)
				return Binding.DoNothing;
			var point = cp.TransformToAncestor(itemsControl).Transform(new Point());

			var sb = (ScrollBar) sv.Template.FindName("PART_VerticalScrollBar", sv);
			var track = (Track) sb.Template.FindName("PART_Track", sb);
			var trackPoint = track.TransformToAncestor(sb).Transform(new Point());

			return trackPoint.Y + ((point.Y * track.ActualHeight) / itemsControl.ActualHeight);
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
