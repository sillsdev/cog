using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SIL.Cog.Presentation.Behaviors
{
	public static class WindowBehaviors
	{
		public static readonly DependencyProperty InitialFocusElementProperty = DependencyProperty.RegisterAttached("InitialFocusElement", typeof(IInputElement), typeof(WindowBehaviors),
			new UIPropertyMetadata(null, InitialFocusElementChanged));

		public static IInputElement GetInitialFocusElement(Window obj)
		{
			return (IInputElement) obj.GetValue(InitialFocusElementProperty);
		}
 
		public static void SetInitialFocusElement(Window obj, IInputElement value)
		{
			obj.SetValue(InitialFocusElementProperty, value);
		}

		private static void InitialFocusElementChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			var window = obj as Window;
			if (window == null)
				return;

			var elem = (IInputElement) e.NewValue;
			if (elem != null)
				window.Activated += window_Activated;
			else
				window.Activated -= window_Activated;
		}

		private static void window_Activated(object sender, EventArgs e)
		{
			var window = (Window) sender;
			IInputElement elem = GetInitialFocusElement(window);
			elem.Focus();
		}

		public static readonly DependencyProperty CloseOnDefaultButtonClickProperty = DependencyProperty.RegisterAttached("CloseOnDefaultButtonClick", typeof(bool), typeof(WindowBehaviors),
			new UIPropertyMetadata(false, CloseOnDefaultButtonClickPropertyChanged));

		public static bool GetCloseOnDefaultButtonClick(Window obj)
		{
			return (bool) obj.GetValue(CloseOnDefaultButtonClickProperty);
		}
 
		public static void SetCloseOnDefaultButtonClick(Window obj, bool value)
		{
			obj.SetValue(CloseOnDefaultButtonClickProperty, value);
		}

		private static void CloseOnDefaultButtonClickPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			var window = obj as Window;
			if (window == null)
				return;

			Button defaultButton = window.FindVisualDescendants<Button>().FirstOrDefault(b => b.IsDefault);
			if (defaultButton == null)
				return;

			if ((bool) e.NewValue)
				defaultButton.Click += defaultButton_Click;
			else
				defaultButton.Click -= defaultButton_Click;
		}

		private static void defaultButton_Click(object sender, RoutedEventArgs e)
		{
			var okButton = (Button) sender;
			Window window = okButton.FindVisualAncestors<Window>().First();
			if (window.Validate())
				window.DialogResult = true;
		}
	}
}
