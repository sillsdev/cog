using SIL.Collections;

namespace SIL.Cog.Domain
{
	public class GlobalSoundCorrespondenceCollection : KeyedBulkObservableList<UnorderedTuple<Segment, Segment>, GlobalSoundCorrespondence>
	{
		protected override UnorderedTuple<Segment, Segment> GetKeyForItem(GlobalSoundCorrespondence item)
		{
			return UnorderedTuple.Create(item.Segment1, item.Segment2);
		}

		public bool TryGetValue(Segment seg1, Segment seg2, out GlobalSoundCorrespondence value)
		{
			return TryGetValue(UnorderedTuple.Create(seg1, seg2), out value);
		}

		public bool Contains(Segment seg1, Segment seg2)
		{
			return Contains(UnorderedTuple.Create(seg1, seg2));
		}

		public bool Remove(Segment seg1, Segment seg2)
		{
			return Remove(UnorderedTuple.Create(seg1, seg2));
		}
	}
}
