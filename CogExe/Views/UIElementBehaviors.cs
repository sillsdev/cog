using System.Windows;

namespace SIL.Cog.Views
{
	public static class UIElementBehaviors
	{
		public static readonly DependencyProperty EnableIsFocusWithinProperty =
			DependencyProperty.RegisterAttached("EnableIsFocusWithin",
												typeof (bool),
												typeof (UIElementBehaviors),
												new UIPropertyMetadata(false, OnEnableIsFocusWithinChanged));

		public static bool GetEnableIsFocusWithin(DependencyObject obj)
		{
			return (bool) obj.GetValue(EnableIsFocusWithinProperty);
		}

		public static void SetEnableIsFocusWithin(DependencyObject obj, bool value)
		{
			obj.SetValue(EnableIsFocusWithinProperty, value);
		}

		private static void OnEnableIsFocusWithinChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var elem = d as UIElement;
			if (elem == null)
				return;

			if (!(e.NewValue is bool))
				return;

			if ((bool) e.NewValue)
			{
				elem.GotFocus += elem_GotFocus;
				elem.LostFocus += elem_LostFocus;
			}
			else
			{
				elem.GotFocus -= elem_GotFocus;
				elem.LostFocus -= elem_LostFocus;
			}
		}

		private static void elem_GotFocus(object sender, RoutedEventArgs e)
		{
			var elem = (UIElement) sender;
			SetIsFocusWithin(elem, true);
		}

		private static void elem_LostFocus(object sender, RoutedEventArgs e)
		{
			var elem = (UIElement) sender;
			SetIsFocusWithin(elem, false);
		}

		private static readonly DependencyPropertyKey IsFocusWithinPropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("IsFocusWithin",
														typeof(bool),
														typeof(UIElementBehaviors),
														new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty IsFocusWithinProperty = IsFocusWithinPropertyKey.DependencyProperty;

		public static bool GetIsFocusWithin(DependencyObject obj)
		{
			return (bool) obj.GetValue(IsFocusWithinProperty);
		}

		private static void SetIsFocusWithin(DependencyObject obj, bool value)
		{
			obj.SetValue(IsFocusWithinPropertyKey, value);
		}
	}
}
