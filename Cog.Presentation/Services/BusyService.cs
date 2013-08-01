using System;
using SIL.Cog.Applications.Services;
using SIL.Cog.Presentation.Views;

namespace SIL.Cog.Presentation.Services
{
	public class BusyService : IBusyService
	{
		public void ShowBusyIndicatorUntilUpdated()
		{
			BusyCursor.DisplayUntilIdle();
		}

		public void ShowBusyIndicator(Action action)
		{
			using (BusyCursor.Display())
				action();
		}
	}
}
