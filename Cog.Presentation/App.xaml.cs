using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Threading;
using SIL.Cog.Presentation.Views;

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

		public App()
		{
			AppDomain.CurrentDomain.UnhandledException += NBug.Handler.UnhandledException;
			System.Windows.Application.Current.DispatcherUnhandledException += NBug.Handler.DispatcherUnhandledException;
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			EventManager.RegisterClassHandler(typeof(TextBox), UIElement.GotFocusEvent, new RoutedEventHandler(TextBox_GotFocus));

			base.OnStartup(e);

			ShutdownMode = ShutdownMode.OnExplicitShutdown;
			var locator = (ViewModelLocator) Resources["Locator"];
			if (locator.Main.Init())
			{
				var mainWindow = new MainWindow();
				ShutdownMode = ShutdownMode.OnLastWindowClose;
				mainWindow.Show();
			}
			else
			{
				Shutdown();
			}
		}

		private void TextBox_GotFocus(object sender, RoutedEventArgs routedEventArgs)
		{
			var textBox = (TextBox) sender;
			textBox.Dispatcher.BeginInvoke(new Action(textBox.SelectAll), DispatcherPriority.Input);
		}
	}
}
