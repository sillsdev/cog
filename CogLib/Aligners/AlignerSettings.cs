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
		private NaturalClass[] _naturalClasses;

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

		public IEnumerable<NaturalClass> NaturalClasses
		{
			get { return _naturalClasses; }
			set
			{
				CheckReadOnly();
				_naturalClasses = value == null ? null : value.ToArray();
			}
		}

		private void CheckReadOnly()
		{
			if (ReadOnly)
				throw new InvalidOperationException("Settings cannot be changed after an AlignerBase object has been created.");
		}
	}
}
