using GalaSoft.MvvmLight.Threading;
using SIL.Cog.Views;

namespace SIL.Cog.Services
{
	public class BusyService : IBusyService
	{
		public void ShowBusyIndicatorUntilUpdated()
		{
			DispatcherHelper.CheckBeginInvokeOnUI(BusyCursor.DisplayUntilIdle);
		}
	}
}
