using System;
using System.Windows.Input;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Threading;

namespace SIL.Cog.Presentation.Views
{
	public static class BusyCursor
	{
		private static bool _isBusyUntilIdle;
		private static int _count;

		public static void DisplayUntilIdle()
		{
			DispatcherHelper.CheckBeginInvokeOnUI(() =>
				{
					Mouse.OverrideCursor = Cursors.Wait;
					if (!_isBusyUntilIdle)
					{
						_isBusyUntilIdle = true;
						DispatcherHelper.UIDispatcher.BeginInvoke(new Action(() =>
							{
								Mouse.OverrideCursor = null;
								_isBusyUntilIdle = false;
							}), DispatcherPriority.ApplicationIdle);
					}
				});
		}

		public static IDisposable Display()
		{
			if (DispatcherHelper.UIDispatcher.CheckAccess())
				StartBusy();
			else
				DispatcherHelper.UIDispatcher.Invoke(new Action(StartBusy));
			return new BusyCursorDisposable();
		}

		private static void StartBusy()
		{
			_count++;
			Mouse.OverrideCursor = Cursors.Wait;
		}

		private static void EndBusy()
		{
			_count--;
			if (!_isBusyUntilIdle && _count == 0)
				Mouse.OverrideCursor = null;
		}

		private class BusyCursorDisposable : IDisposable
		{
			public void Dispose()
			{
				if (DispatcherHelper.UIDispatcher.CheckAccess())
					EndBusy();
				else
					DispatcherHelper.UIDispatcher.Invoke(new Action(EndBusy));
			}
		}
	}
}
