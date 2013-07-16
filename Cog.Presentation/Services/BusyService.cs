using GalaSoft.MvvmLight.Threading;
using SIL.Cog.Applications.Services;
using SIL.Cog.Presentation.Views;

namespace SIL.Cog.Presentation.Services
{
	public class BusyService : IBusyService
	{
		public void ShowBusyIndicatorUntilUpdated()
		{
			DispatcherHelper.CheckBeginInvokeOnUI(BusyCursor.DisplayUntilIdle);
		}
	}
}
