using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace SIL.Cog.Views
{
	public class OverrideCursor : IDisposable
	{
		private static readonly Stack<Cursor> Cursors = new Stack<Cursor>(); 

		public OverrideCursor(Cursor cursor)
		{
			Cursors.Push(cursor);
			if (Mouse.OverrideCursor != cursor)
				Mouse.OverrideCursor = cursor;
		}

		public void Dispose()
		{
			Cursors.Pop();

			Cursor cursor = Cursors.Count > 0 ? Cursors.Peek() : null;
			if (cursor != Mouse.OverrideCursor)
				Mouse.OverrideCursor = cursor;
		}
	}
}
