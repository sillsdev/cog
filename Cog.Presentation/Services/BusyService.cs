﻿using System;
using SIL.Cog.Application.Services;
using SIL.Cog.Presentation.Views;

namespace SIL.Cog.Presentation.Services
{
	public class BusyService : IBusyService
	{
		public void ShowBusyIndicatorUntilFinishDrawing()
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
