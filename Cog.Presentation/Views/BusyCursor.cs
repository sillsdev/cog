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
		}

		public static IDisposable Display()
		{
			_count++;
			Mouse.OverrideCursor = Cursors.Wait;
			return new BusyCursorDisposable();
		}

		private class BusyCursorDisposable : IDisposable
		{
			public void Dispose()
			{
				_count--;
				if (!_isBusyUntilIdle && _count == 0)
					Mouse.OverrideCursor = null;
			}
		}
	}
}
