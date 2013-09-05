using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace SIL.Cog.Presentation.Behaviors
{
	public static class UIElementBehaviors
	{
		static UIElementBehaviors()
		{
			EventManager.RegisterClassHandler(typeof (UIElement), UIElement.GotFocusEvent, new RoutedEventHandler(elem_GotFocus), true);
			EventManager.RegisterClassHandler(typeof (UIElement), UIElement.LostFocusEvent, new RoutedEventHandler(elem_LostFocus), true);
			CommandManager.RegisterClassCommandBinding(typeof(UIElement), new CommandBinding(ApplicationCommands.Help, ExecuteHelp, CanExecuteHelp));
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

		public static readonly DependencyProperty HelpFileProperty = DependencyProperty.RegisterAttached("HelpFile", typeof(string), typeof(UIElementBehaviors),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
 
		public static string GetHelpFile(UIElement obj)
		{
			return (string) obj.GetValue(HelpFileProperty);
		}
 
		public static void SetHelpFile(UIElement obj, string value)
		{
			obj.SetValue(HelpFileProperty, value);
		}

		public static readonly DependencyProperty HelpTopicProperty = DependencyProperty.RegisterAttached("HelpTopic", typeof(string), typeof(UIElementBehaviors),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
 
		public static string GetHelpTopic(UIElement obj)
		{
			return (string) obj.GetValue(HelpTopicProperty);
		}
 
		public static void SetHelpTopic(UIElement obj, string value)
		{
			obj.SetValue(HelpTopicProperty, value);
		}
 
		private static void CanExecuteHelp(object sender, CanExecuteRoutedEventArgs args)
		{
			var elem = sender as UIElement;
			if (elem == null)
				return;

			string filename = GetHelpFile(elem);
			if (!string.IsNullOrEmpty(filename))
				args.CanExecute = true;
		}
 
		private static void ExecuteHelp(object sender, ExecutedRoutedEventArgs args)
		{
			// Call ShowHelp.
			var elem = sender as UIElement;
			if (elem == null)
				return;

			string filename = GetHelpFile(elem);
			if (!string.IsNullOrEmpty(filename))
			{
				if (!Path.IsPathRooted(filename))
				{
					string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
					if (!string.IsNullOrEmpty(dir))
						filename = Path.Combine(dir, filename);
				}
				string topic = GetHelpTopic(elem);
				IntPtr windowHandle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
				if (string.IsNullOrEmpty(topic))
					HtmlHelp(windowHandle, filename, 1, null);
				else
					HtmlHelp(windowHandle, filename, 0, topic);
			}
		}

		[DllImport("hhctrl.ocx", CharSet = CharSet.Auto)]
		private static extern IntPtr HtmlHelp(IntPtr hWndCaller, string helpFile, int command, string data);
	}
}
