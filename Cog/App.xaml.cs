using GalaSoft.MvvmLight.Threading;

namespace SIL.Cog
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
	}
}
