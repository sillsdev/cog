using System.Collections.Generic;
using System.Linq;
using SIL.Collections;

namespace SIL.Cog
{
	public class SoundChange : NotifyPropertyChangedBase
	{
		private readonly VarietyPair _varietyPair;
		private readonly SoundChangeLhs _lhs;
		private readonly Dictionary<NSegment, double> _probabilities;

		internal SoundChange(VarietyPair varietyPair, SoundChangeLhs lhs)
		{
			_varietyPair = varietyPair;
			_lhs = lhs;
			_probabilities = new Dictionary<NSegment, double>();
		}

		public SoundChangeLhs Lhs
		{
			get { return _lhs; }
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

				return (1.0 - _probabilities.Values.Sum()) / (_varietyPair.SoundChanges.PossibleCorrespondenceCount - _probabilities.Count);
			}

			set
			{
				_probabilities[correspondence] = value;
				OnPropertyChanged("Item[]");
			}
		}

		public void Reset()
		{
			_probabilities.Clear();
			OnPropertyChanged("Item[]");
		}
	}
}
