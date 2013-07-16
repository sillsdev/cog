using System;
using System.Windows;

namespace SIL.Cog.Presentation.Controls
{
	public class ContentSizeChangedEventArgs : EventArgs
	{
		private readonly Size _contentSize;

		public ContentSizeChangedEventArgs(Size contentSize)
		{
			_contentSize = contentSize;
		}

		public Size ContentSize
		{
			get { return _contentSize; }
		}
	}
}
