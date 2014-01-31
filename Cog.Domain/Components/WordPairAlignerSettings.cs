using System;
using System.Collections.Generic;
using System.Linq;
using SIL.Machine.SequenceAlignment;

namespace SIL.Cog.Domain.Components
{
	public sealed class WordPairAlignerSettings
	{
		private AlignmentMode _mode;
		private bool _expansionCompressionEnabled;
		private SoundClass[] _contextualSoundClasses;

		internal bool ReadOnly { get; set; }

		public AlignmentMode Mode
		{
			get { return _mode; }
			set
			{
				CheckReadOnly();
				_mode = value;
			}
		}

		public bool ExpansionCompressionEnabled
		{
			get { return _expansionCompressionEnabled; }
			set
			{
				CheckReadOnly();
				_expansionCompressionEnabled = value;
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
				throw new InvalidOperationException("Settings cannot be changed after an Aligner object has been created.");
		}
	}
}
