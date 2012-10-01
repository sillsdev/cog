using System.Windows;

namespace SIL.Cog.Views
{
	public static class FrameworkElementBehaviors
	{
		public static DependencyProperty IsContextMenuOpenProperty = DependencyProperty.RegisterAttached("IsContextMenuOpen",
				 typeof(bool), typeof(FrameworkElementBehaviors), new PropertyMetadata(false));

		public static bool GetIsContextMenuOpen(FrameworkElement target)
		{
			return (bool) target.GetValue(IsContextMenuOpenProperty);
		}

		public static void SetIsContextMenuOpen(FrameworkElement target, bool value)
		{
			target.SetValue(IsContextMenuOpenProperty, value);
		}
	}
}
