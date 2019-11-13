using System.Windows;
using GalaSoft.MvvmLight.Threading;
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

		protected override void OnStartup(StartupEventArgs e)
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
		}
	}
}
