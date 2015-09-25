using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SIL.Cog.Presentation.Behaviors
{
	public static class ComboBoxBehaviors
	{
		public static readonly DependencyProperty AutoSizeProperty = DependencyProperty.RegisterAttached("AutoSize", typeof(bool), typeof(ComboBoxBehaviors),
			new UIPropertyMetadata(false, OnAutoSizeChanged));

		private static void OnAutoSizeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			var comboBox = (ComboBox) obj;
			if ((bool) e.NewValue)
			{
				SetWidthToFit(comboBox);
				ItemPropertyChangedListener.Subscribe(comboBox, comboBox.Items, comboBox.DisplayMemberPath, ComboBox_ItemsChanged);
			}
			else
			{
				ItemPropertyChangedListener.Unsubscribe(comboBox);
			}
		}

		private static void ComboBox_ItemsChanged(object obj)
		{
			var comboBox = (ComboBox) obj;
			SetWidthToFit(comboBox);
		}

		private static void SetWidthToFit(ComboBox comboBox)
		{
			double maxWidth = 0;
			foreach (object item in comboBox.Items)
			{
				object[] values = item.GetPropertyValues(comboBox.DisplayMemberPath).ToArray();

				object value = values.Length == 0 ? item : values[values.Length - 1];
				var formattedText = new FormattedText(value.ToString(), CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
					new Typeface(comboBox.FontFamily, comboBox.FontStyle, comboBox.FontWeight, comboBox.FontStretch), comboBox.FontSize, comboBox.Foreground);
				if (formattedText.Width > maxWidth)
					maxWidth = formattedText.Width;
			}
			comboBox.Width = maxWidth + 28;
		}

		public static void SetAutoSize(ComboBox comboBox, bool value)
		{
			comboBox.SetValue(AutoSizeProperty, value);
		}

		public static bool GetAutoSize(ComboBox comboBox)
		{
			return (bool) comboBox.GetValue(AutoSizeProperty);
		}
	}
}
