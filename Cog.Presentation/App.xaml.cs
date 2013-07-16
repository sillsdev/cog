using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Threading;

namespace SIL.Cog.Presentation
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App
	{
		static App()
		{
			DispatcherHelper.Initialize();
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			EventManager.RegisterClassHandler(typeof(TextBox), UIElement.GotFocusEvent, new RoutedEventHandler(TextBox_GotFocus));

			base.OnStartup(e);
		}

		private void TextBox_GotFocus(object sender, RoutedEventArgs routedEventArgs)
		{
			var textBox = (TextBox) sender;
			textBox.Dispatcher.BeginInvoke(new Action(textBox.SelectAll), DispatcherPriority.Input);
		}
	}
}
