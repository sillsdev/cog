using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace SIL.Cog.Views
{
	public static class TextBoxBehaviors
	{
		public static DependencyProperty IsDirtyEnabledProperty = DependencyProperty.RegisterAttached("IsDirtyEnabled",
				 typeof(bool), typeof(TextBoxBehaviors), new PropertyMetadata(false, OnIsDirtyEnabledChanged));

		public static bool GetIsDirtyEnabled(TextBox target)
		{
			return (bool) target.GetValue(IsDirtyEnabledProperty);
		}

		public static void SetIsDirtyEnabled(TextBox target, bool value)
		{
			target.SetValue(IsDirtyEnabledProperty, value);
		}

		public static DependencyProperty ShowErrorTemplateProperty = DependencyProperty.RegisterAttached("ShowErrorTemplate",
				 typeof(bool), typeof(TextBoxBehaviors), new PropertyMetadata(false));

		public static bool GetShowErrorTemplate(TextBox target)
		{
			return (bool) target.GetValue(ShowErrorTemplateProperty);
		}

		public static void SetShowErrorTemplate(TextBox target, bool value)
		{
			target.SetValue(ShowErrorTemplateProperty, value);
		}

		private static void OnIsDirtyEnabledChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
			var textBox = (TextBox) dependencyObject;
			if (textBox == null)
				return;

			if ((bool) e.NewValue)
			{
				textBox.LostFocus += TextBoxLostFocus;
				textBox.PreviewKeyDown += TextBoxKeyDown;
			}
			else
			{
				textBox.LostFocus -= TextBoxLostFocus;
				textBox.PreviewKeyDown -= TextBoxKeyDown;
			}
		}

		private static void TextBoxKeyDown(object sender, KeyEventArgs e)
		{
			var textBox = (TextBox) sender;
			if (e.Key == Key.Enter)
			{
				ShowErrorTemplate(textBox);
				BindingExpression binding = BindingOperations.GetBindingExpression(textBox, TextBox.TextProperty);

				if (binding != null)
					binding.UpdateSource();
			}
		}

		private static void TextBoxLostFocus(object sender, RoutedEventArgs e)
		{
			var textBox = (TextBox) sender;
			ShowErrorTemplate(textBox);
		}

		private static void ShowErrorTemplate(TextBox textBox)
		{
			if ((bool) textBox.GetValue(ShowErrorTemplateProperty) == false)
				textBox.SetValue(ShowErrorTemplateProperty, true);
		}
	}
}
