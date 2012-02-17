using System.Collections.Generic;
using System.Linq;
using SIL.Machine;

namespace SIL.Cog
{
	public class SoundChange
	{
		private readonly int _possibleCorrespondenceCount;
		private readonly NaturalClass _leftEnv;
		private readonly NSegment _target;
		private readonly NaturalClass _rightEnv;
		private readonly Dictionary<NSegment, double> _probabilities;

		internal SoundChange(int possibleCorrespondenceCount, NaturalClass leftEnv, NSegment target, NaturalClass rightEnv)
		{
			_possibleCorrespondenceCount = possibleCorrespondenceCount;
			_leftEnv = leftEnv;
			_target = target;
			_rightEnv = rightEnv;
			_probabilities = new Dictionary<NSegment, double>();
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

		public IReadOnlyCollection<NSegment> ObservedCorrespondences
		{
			get { return _probabilities.Keys.AsReadOnlyCollection(); }
		}

		public double this[NSegment correspondence]
		{
			get
			{
				double probability;
				if (_probabilities.TryGetValue(correspondence, out probability))
					return probability;

				return (1.0 - _probabilities.Values.Sum()) / (_possibleCorrespondenceCount - _probabilities.Count);
			}

			set
			{
				_probabilities[correspondence] = value;
			}
		}

		public void Reset()
		{
			_probabilities.Clear();
		}

		public override string ToString()
		{
			string targetStr = _target.ToString();
			if (_leftEnv != null && _rightEnv != null)
				return string.Format("{0} -> ? / {1} _ {2}", targetStr, _leftEnv, _rightEnv);
			if (_leftEnv == null && _rightEnv == null)
				return string.Format("{0} -> ?", targetStr);
			if (_leftEnv == null)
				return string.Format("{0} -> ? / _ {1}", targetStr, _rightEnv);

			return string.Format("{0} -> ? / {1} _", targetStr, _leftEnv);
		}
	}
}
