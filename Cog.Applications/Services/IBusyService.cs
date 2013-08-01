using System;

namespace SIL.Cog.Applications.Services
{
	public interface IBusyService
	{
		void ShowBusyIndicatorUntilUpdated();
		void ShowBusyIndicator(Action action);
	}
}
