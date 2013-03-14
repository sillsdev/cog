using System;
using System.Collections.Generic;
using System.Linq;

namespace SIL.Cog.Aligners
{
	public enum AlignerMode
	{
		Global = 0,
		SemiGlobal,
		HalfLocal,
		Local
	}

	public sealed class AlignerSettings
	{
		private AlignerMode _mode;
		private bool _disableExpansionCompression;
		private SoundClass[] _contextualSoundClasses;

		internal bool ReadOnly { get; set; }

		public AlignerMode Mode
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

		public IEnumerable<SoundClass> ContextualSoundClasses
		{
			get
			{
				if (_contextualSoundClasses == null)
					return Enumerable.Empty<SoundClass>();
				return _contextualSoundClasses;
			}
			set
			{
				CheckReadOnly();
				_contextualSoundClasses = value == null ? null : value.ToArray();
			}
		}

		private void CheckReadOnly()
		{
			if (ReadOnly)
				throw new InvalidOperationException("Settings cannot be changed after an AlignerBase object has been created.");
		}
	}
}
