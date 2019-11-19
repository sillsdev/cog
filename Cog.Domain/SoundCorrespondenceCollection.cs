using SIL.ObjectModel;

namespace SIL.Cog.Domain
{
	public class SoundCorrespondenceCollection : KeyedBulkObservableList<UnorderedTuple<Segment, Segment>, SoundCorrespondence>
	{
		protected override UnorderedTuple<Segment, Segment> GetKeyForItem(SoundCorrespondence item)
		{
			return UnorderedTuple.Create(item.Segment1, item.Segment2);
		}

		public bool TryGet(Segment seg1, Segment seg2, out SoundCorrespondence value)
		{
			return TryGet(UnorderedTuple.Create(seg1, seg2), out value);
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
