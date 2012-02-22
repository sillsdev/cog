using System;

namespace SIL.Cog
{
	public enum EditDistanceMode
	{
		Global = 0,
		SemiGlobal,
		HalfLocal,
		Local
	}

	public sealed class EditDistanceSettings
	{
		private EditDistanceMode _mode;
		private bool _disableExpansionCompression;

		internal bool ReadOnly { get; set; }

		public EditDistanceMode Mode
		{
			get { return _mode; }
			set
			{
				CheckReadOnly();
				_mode = value;
			}
		}

		public bool DisableExpansionCompression
		{
			get { return _disableExpansionCompression; }
			set
			{
				CheckReadOnly();
				_disableExpansionCompression = value;
			}
		}

		private void CheckReadOnly()
		{
			if (ReadOnly)
				throw new InvalidOperationException("Settings cannot be changed after an EditDistance object has been created.");
		}
	}
}
