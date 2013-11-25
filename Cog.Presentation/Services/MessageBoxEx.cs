using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;

namespace SIL.Cog.Presentation.Services
{
	public class MessageBoxEx
	{
		private static IntPtr _owner;
		private static readonly HookProc _hookProc;
		private static IntPtr _hHook;

		public static MessageBoxResult Show(string text)
		{
			return MessageBox.Show(text);
		}

		public static MessageBoxResult Show(string text, string caption)
		{
			return MessageBox.Show(text, caption);
		}

		public static MessageBoxResult Show(string text, string caption, MessageBoxButton buttons)
		{
			return MessageBox.Show(text, caption, buttons);
		}

		public static MessageBoxResult Show(string text, string caption, MessageBoxButton buttons, MessageBoxImage icon)
		{
			return MessageBox.Show(text, caption, buttons, icon);
		}

		public static MessageBoxResult Show(string text, string caption, MessageBoxButton buttons, MessageBoxImage icon, MessageBoxResult defResult)
		{
			return MessageBox.Show(text, caption, buttons, icon, defResult);
		}

		public static MessageBoxResult Show(string text, string caption, MessageBoxButton buttons, MessageBoxImage icon, MessageBoxResult defResult, MessageBoxOptions options)
		{        
			return MessageBox.Show(text, caption, buttons, icon, defResult, options);
		}

		public static MessageBoxResult Show(Window owner, string text)
		{
			_owner = new WindowInteropHelper(owner).Handle;
			Initialize();
			return MessageBox.Show(owner, text);
		}

		public static MessageBoxResult Show(Window owner, string text, string caption)
		{
			_owner = new WindowInteropHelper(owner).Handle;
			Initialize();
			return MessageBox.Show(owner, text, caption);
		}

		public static MessageBoxResult Show(Window owner, string text, string caption, MessageBoxButton buttons)
		{
			_owner = new WindowInteropHelper(owner).Handle;
			Initialize();
			return MessageBox.Show(owner, text, caption, buttons);
		}

		public static MessageBoxResult Show(Window owner, string text, string caption, MessageBoxButton buttons, MessageBoxImage icon)
		{
			_owner = new WindowInteropHelper(owner).Handle;
			Initialize();
			return MessageBox.Show(owner, text, caption, buttons, icon);
		}

		public static MessageBoxResult Show(Window owner, string text, string caption, MessageBoxButton buttons, MessageBoxImage icon, MessageBoxResult defResult)
		{
			_owner = new WindowInteropHelper(owner).Handle;
			Initialize();
			return MessageBox.Show(owner, text, caption, buttons, icon, defResult);
		}

		public static MessageBoxResult Show(Window owner, string text, string caption, MessageBoxButton buttons, MessageBoxImage icon, MessageBoxResult defResult, MessageBoxOptions options)
		{
			_owner = new WindowInteropHelper(owner).Handle;
			Initialize();
			return MessageBox.Show(owner, text, caption, buttons, icon,
									defResult, options);
		}

		public delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

		public delegate void TimerProc(IntPtr hWnd, uint uMsg, UIntPtr nIDEvent, uint dwTime);

		public const int WH_CALLWNDPROCRET = 12;

		public enum CbtHookAction : int
		{
			HCBT_MOVESIZE = 0,
			HCBT_MINMAX = 1,
			HCBT_QS = 2,
			HCBT_CREATEWND = 3,
			HCBT_DESTROYWND = 4,
			HCBT_ACTIVATE = 5,
			HCBT_CLICKSKIPPED = 6,
			HCBT_KEYSKIPPED = 7,
			HCBT_SYSCOMMAND = 8,
			HCBT_SETFOCUS = 9
		}

		[DllImport("user32.dll")]
		private static extern bool GetWindowRect(IntPtr hWnd, ref Rectangle lpRect);

		[DllImport("user32.dll")]
		private static extern int MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

		[DllImport("user32.dll")]
		public static extern UIntPtr SetTimer(IntPtr hWnd, UIntPtr nIDEvent, uint uElapse, TimerProc lpTimerFunc);

		[DllImport("user32.dll")]
		public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		public static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

		[DllImport("user32.dll")]
		public static extern int UnhookWindowsHookEx(IntPtr idHook);

		[DllImport("user32.dll")]
		public static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		public static extern int GetWindowTextLength(IntPtr hWnd);

		[DllImport("user32.dll")]
		public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int maxLength);

		[DllImport("user32.dll")]
		public static extern int EndDialog(IntPtr hDlg, IntPtr nResult);

		[DllImport("kernel32.dll")]
		public static extern int GetCurrentThreadId();

		[StructLayout(LayoutKind.Sequential)]
		public struct CWPRETSTRUCT
		{
			public IntPtr lResult;
			public IntPtr lParam;
			public IntPtr wParam;
			public uint message;
			public IntPtr hwnd;
		} ;

		static MessageBoxEx()
		{
			_hookProc = MessageBoxHookProc;
			_hHook = IntPtr.Zero;
		}

		private static void Initialize()
		{
			if (_hHook != IntPtr.Zero)
			{
				throw new NotSupportedException("multiple calls are not supported");
			}

			if (_owner != null)
			{
				_hHook = SetWindowsHookEx(WH_CALLWNDPROCRET, _hookProc, IntPtr.Zero, GetCurrentThreadId());
			}
		}

		private static IntPtr MessageBoxHookProc(int nCode, IntPtr wParam, IntPtr lParam)
		{
			if (nCode < 0)
			{
				return CallNextHookEx(_hHook, nCode, wParam, lParam);
			}

			var msg = (CWPRETSTRUCT)Marshal.PtrToStructure(lParam, typeof(CWPRETSTRUCT));
			IntPtr hook = _hHook;

			if (msg.message == (int) CbtHookAction.HCBT_ACTIVATE)
			{
				try
				{
					CenterWindow(msg.hwnd);
				}
				finally
				{
					UnhookWindowsHookEx(_hHook);
					_hHook = IntPtr.Zero;
				}
			}

			return CallNextHookEx(hook, nCode, wParam, lParam);
		}

		private static void CenterWindow(IntPtr hChildWnd)
		{
			var recChild = new Rectangle(0, 0, 0, 0);
			GetWindowRect(hChildWnd, ref recChild);

			int width = recChild.Width - recChild.X;
			int height = recChild.Height - recChild.Y;

			var recParent = new Rectangle(0, 0, 0, 0);
			GetWindowRect(_owner, ref recParent);

			var ptCenter = new System.Drawing.Point(recParent.X + ((recParent.Width - recParent.X) / 2), recParent.Y + ((recParent.Height - recParent.Y) / 2));
			var ptStart = new System.Drawing.Point(ptCenter.X - (width / 2), ptCenter.Y - (height / 2));
			ptStart.X = (ptStart.X < 0) ? 0 : ptStart.X;
			ptStart.Y = (ptStart.Y < 0) ? 0 : ptStart.Y;

			MoveWindow(hChildWnd, ptStart.X, ptStart.Y, width, height, false);
		}
	}
}
