using System;

namespace SIL.Cog
{
	public class SoundContext : IEquatable<SoundContext>
	{
		private readonly SoundClass _leftEnv;
		private readonly Ngram _target;
		private readonly SoundClass _rightEnv;

		public SoundContext(Ngram target)
			: this(null, target, null)
		{
		}

		public SoundContext(SoundClass leftEnv, Ngram target)
			: this(leftEnv, target, null)
		{
		}

		public SoundContext(Ngram target, SoundClass rightEnv)
			: this(null, target, rightEnv)
		{
		}

		public SoundContext(SoundClass leftEnv, Ngram target, SoundClass rightEnv)
		{
			_leftEnv = leftEnv;
			_target = target;
			_rightEnv = rightEnv;
		}

		public SoundClass LeftEnvironment
		{
			get { return _leftEnv; }
		}

		public Ngram Target
		{
			get { return _target; }
		}

		public SoundClass RightEnvironment
		{
			get { return _rightEnv; }
		}

		public bool Equals(SoundContext other)
		{
			if (other == null)
				return false;

			return _leftEnv == other._leftEnv && _target.Equals(other._target) && _rightEnv == other._rightEnv;
		}

		public override bool Equals(object obj)
		{
			var lhs = obj as SoundContext;
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
			if (_leftEnv != null && _rightEnv == null)
				return string.Format("{0} -> ? / [{1}] _", targetStr, _leftEnv.Name);
			if (_leftEnv == null && _rightEnv != null)
				return string.Format("{0} -> ? / _ [{1}]", targetStr, _rightEnv.Name);
			return string.Format("{0} -> ?", targetStr);
		}
	}
}
