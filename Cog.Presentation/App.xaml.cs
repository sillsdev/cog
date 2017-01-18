using System.Windows;
using GalaSoft.MvvmLight.Threading;
using Microsoft.HockeyApp;
using SIL.Cog.Presentation.Properties;
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

		protected override async void OnStartup(StartupEventArgs e)
		{
			if (Settings.Default.NeedsUpgrade)
			{
				Settings.Default.Upgrade();
				Settings.Default.NeedsUpgrade = false;
				Settings.Default.Save();
			}

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

			HockeyClient.Current.Configure("ad6db2188aaa413fa2fa5055af9fcfd3");
			await HockeyClient.Current.SendCrashesAsync();
		}
	}
}
