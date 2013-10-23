using System;

namespace SIL.Cog.Applications.Services
{
	public interface IBusyService
	{
		void ShowBusyIndicatorUntilFinishDrawing();
		void ShowBusyIndicator(Action action);
	}
}
