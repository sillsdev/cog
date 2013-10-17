using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace SIL.Cog.Presentation.Converters
{
	public class ListBoxScrollbarMarkerPositionConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			object item = values[0];
			var listBox = (ListBox) values[1];

			var sv = listBox.FindVisualChild<ScrollViewer>();

			int index = listBox.Items.IndexOf(item);

			int i = 0;
			FrameworkElement cp = null;
			while (cp == null && i < listBox.Items.Count)
				cp = (FrameworkElement) listBox.ItemContainerGenerator.ContainerFromItem(listBox.Items[i++]);

			if (cp == null)
				return Binding.DoNothing;

			// assume that all items are the same height
			double y = index * cp.ActualHeight;
			double height = listBox.Items.Count * cp.ActualHeight;

			var sb = (ScrollBar) sv.Template.FindName("PART_VerticalScrollBar", sv);
			var track = (Track) sb.Template.FindName("PART_Track", sb);
			var trackPoint = track.TransformToAncestor(sb).Transform(new Point());
			return trackPoint.Y + ((y * track.ActualHeight) / height);
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
