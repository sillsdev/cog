using SIL.Collections;

namespace SIL.Cog
{
	public class SoundChangeCollection : KeyedObservableCollection<SoundChangeLhs, SoundChange>
	{
		private readonly VarietyPair _varietyPair;

		internal SoundChangeCollection(VarietyPair pair)
		{
			_varietyPair = pair;
		}

		public double DefaultCorrespondenceProbability
		{
			get { return 1.0 / PossibleCorrespondenceCount; }
		}

		public int PossibleCorrespondenceCount
		{
			get
			{
				int segmentCount = _varietyPair.Variety2.Segments.Count;
				return (segmentCount * segmentCount) + segmentCount + 1;
			}
		}

		public SoundChange Add(SoundChangeLhs lhs)
		{
			var soundChange = new SoundChange(_varietyPair, lhs);
			Add(soundChange);
			return soundChange;
		}
	}
}
