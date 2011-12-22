using System.Collections.Generic;
using System.Linq;

namespace SIL.Cog
{
	public class SoundChange
	{
		private readonly int _correspondenceCount;
		private readonly NaturalClass _leftEnv;
		private readonly string _target;
		private readonly NaturalClass _rightEnv;
		private readonly Dictionary<string, double> _correspondenceProbs;

		internal SoundChange(int phonemeCount, NaturalClass leftEnv, string target, NaturalClass rightEnv)
		{
			_correspondenceCount = (phonemeCount * phonemeCount) + phonemeCount;
			_leftEnv = leftEnv;
			_target = target;
			_rightEnv = rightEnv;
			_correspondenceProbs = new Dictionary<string, double>();
		}

		public int CorrespondenceCount
		{
			get { return _correspondenceCount; }
		}

		public NaturalClass LeftEnvironment
		{
			get { return _leftEnv; }
		}

		public string Target
		{
			get { return _target; }
		}

		public NaturalClass RightEnvironment
		{
			get { return _rightEnv; }
		}

		public IEnumerable<string> ObservedCorrespondences
		{
			get { return _correspondenceProbs.Keys; }
		}

		public int ObservedCorrespondenceCount
		{
			get { return _correspondenceProbs.Count; }
		}

		public double this[string correspondence]
		{
			get
			{
				double prob;
				if (!_correspondenceProbs.TryGetValue(correspondence, out prob))
				{
					double totalProb = _correspondenceProbs.Values.Aggregate(0.0, (total, p) => total + p);
					prob = (1.0 - totalProb) / (_correspondenceCount - _correspondenceProbs.Count);
				}
				return prob;
			}

			set { _correspondenceProbs[correspondence] = value; }
		}

		public void Reset()
		{
			_correspondenceProbs.Clear();
		}
	}
}
