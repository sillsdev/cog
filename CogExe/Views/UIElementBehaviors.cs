using System.Windows;

namespace SIL.Cog.Views
{
	public static class UIElementBehaviors
	{
		static UIElementBehaviors()
		{
			EventManager.RegisterClassHandler(typeof (UIElement), UIElement.GotFocusEvent, new RoutedEventHandler(elem_GotFocus), true);
			EventManager.RegisterClassHandler(typeof (UIElement), UIElement.LostFocusEvent, new RoutedEventHandler(elem_LostFocus), true);
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
