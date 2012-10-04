using System;

namespace SIL.Cog
{
	public class SoundChangeLhs : IEquatable<SoundChangeLhs>
	{
		private readonly NaturalClass _leftEnv;
		private readonly NSegment _target;
		private readonly NaturalClass _rightEnv;

		public SoundChangeLhs(NSegment target)
			: this(null, target, null)
		{
		}

		public SoundChangeLhs(NaturalClass leftEnv, NSegment target)
			: this(leftEnv, target, null)
		{
		}

		public SoundChangeLhs(NSegment target, NaturalClass rightEnv)
			: this(null, target, rightEnv)
		{
		}

		public SoundChangeLhs(NaturalClass leftEnv, NSegment target, NaturalClass rightEnv)
		{
			_leftEnv = leftEnv;
			_target = target;
			_rightEnv = rightEnv;
		}

		public NaturalClass LeftEnvironment
		{
			get { return _leftEnv; }
		}

		public NSegment Target
		{
			get { return _target; }
		}

		public NaturalClass RightEnvironment
		{
			get { return _rightEnv; }
		}

		public bool Equals(SoundChangeLhs other)
		{
			return _leftEnv == other._leftEnv && _target.Equals(other._target) && _rightEnv == other._rightEnv;
		}

		public override bool Equals(object obj)
		{
			var lhs = obj as SoundChangeLhs;
			return obj != null && Equals(lhs);
		}

		public override int GetHashCode()
		{
			int code = 23;
			code = code * 31 + (_leftEnv == null ? 0 : _leftEnv.GetHashCode());
			code = code * 31 + _target.GetHashCode();
			code = code * 31 + (_rightEnv == null ? 0 : _rightEnv.GetHashCode());
			return code;
		}

		public override string ToString()
		{
			string targetStr = _target.ToString();
			if (_leftEnv != null && _rightEnv != null)
				return string.Format("{0} -> ? / [{1}] _ [{2}]", targetStr, _leftEnv.Name, _rightEnv.Name);
			if (_leftEnv == null && _rightEnv == null)
				return string.Format("{0} -> ?", targetStr);
			if (_leftEnv == null)
				return string.Format("{0} -> ? / _ [{1}]", targetStr, _rightEnv.Name);

			return string.Format("{0} -> ? / [{1}] _", targetStr, _leftEnv.Name);
		}
	}
}
