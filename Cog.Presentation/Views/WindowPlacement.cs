using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Xml;
using System.Xml.Serialization;

namespace SIL.Cog.Presentation.Views
{
	// RECT structure required by WINDOWPLACEMENT structure
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct RECT
	{
		public int Left;
		public int Top;
		public int Right;
		public int Bottom;

		public RECT(int left, int top, int right, int bottom)
		{
			Left = left;
			Top = top;
			Right = right;
			Bottom = bottom;
		}
	}

	// POINT structure required by WINDOWPLACEMENT structure
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct POINT
	{
		public int X;
		public int Y;

		public POINT(int x, int y)
		{
			X = x;
			Y = y;
		}
	}

	// WINDOWPLACEMENT stores the position, size, and state of a window
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct WINDOWPLACEMENT
	{
		public int length;
		public int flags;
		public int showCmd;
		public POINT minPosition;
		public POINT maxPosition;
		public RECT normalPosition;
	}

	public static class WindowPlacement
	{
		private static readonly Encoding Encoding = new UTF8Encoding();
		private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(WINDOWPLACEMENT));

		[DllImport("user32.dll")]
		private static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

		[DllImport("user32.dll")]
		private static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);

		private const int SW_SHOWNORMAL = 1;
		private const int SW_SHOWMINIMIZED = 2;

		public static void SetPlacement(Window window, string placementXml)
		{
			IntPtr windowHandle = new WindowInteropHelper(window).Handle;
			if (string.IsNullOrEmpty(placementXml))
			{
				return;
			}

			byte[] xmlBytes = Encoding.GetBytes(placementXml);

			try
			{
				WINDOWPLACEMENT placement;
				using (var memoryStream = new MemoryStream(xmlBytes))
				{
					placement = (WINDOWPLACEMENT) Serializer.Deserialize(memoryStream);
				}

				placement.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
				placement.flags = 0;
				placement.showCmd = (placement.showCmd == SW_SHOWMINIMIZED ? SW_SHOWNORMAL : placement.showCmd);
				SetWindowPlacement(windowHandle, ref placement);
			}
			catch (InvalidOperationException)
			{
				// Parsing placement XML failed. Fail silently.
			}
		}

		public static string GetPlacement(Window window)
		{
			IntPtr windowHandle = new WindowInteropHelper(window).Handle;

			WINDOWPLACEMENT placement;
			GetWindowPlacement(windowHandle, out placement);

			using (var memoryStream = new MemoryStream())
			{
				using (var xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8))
				{
					Serializer.Serialize(xmlTextWriter, placement);
					byte[] xmlBytes = memoryStream.ToArray();
					return Encoding.GetString(xmlBytes);
				}
			}
		}
	}
}
