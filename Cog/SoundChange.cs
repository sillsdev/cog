using System;
using System.Collections.Generic;
using System.Linq;

namespace SIL.Cog
{
	public class SoundChange
	{
		private readonly string _target;
		private readonly Dictionary<string, Correspondence> _correspondences;
		private readonly int _possibleCorrespondenceCount;

		public SoundChange(string target, int possibleCorrespondenceCount)
		{
			_target = target;
			_possibleCorrespondenceCount = possibleCorrespondenceCount;
			_correspondences = new Dictionary<string, Correspondence>();
		}

		public string Target
		{
			get { return _target; }
		}

		public Correspondence GetCorrespondence(string phoneme)
		{
			Correspondence corr;
			if (_correspondences.TryGetValue(phoneme, out corr))
				return corr;

			corr = new Correspondence(phoneme) {Probability = (_target == phoneme ? 6.0 : 1.1 / _possibleCorrespondenceCount) / 7.1};
			_correspondences[phoneme] = corr;
			return corr;
		}
	}
}
