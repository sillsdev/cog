using SIL.ObjectModel;

namespace SIL.Cog.Domain
{
	public class VarietyPairCollection : BulkObservableList<VarietyPair>
	{
		protected override void InsertItem(int index, VarietyPair item)
		{
			base.InsertItem(index, item);
			AddVarietyPairToVarieties(item);
		}

		protected override void ClearItems()
		{
			foreach (VarietyPair vp in this)
			{
				vp.Variety1.VarietyPairs.VarietyPairsCleared();
				vp.Variety2.VarietyPairs.VarietyPairsCleared();
			}

			base.ClearItems();
		}

		protected override void RemoveItem(int index)
		{
			RemoveVarietyPairFromVarieties(index);
			base.RemoveItem(index);
		}

		protected override void SetItem(int index, VarietyPair item)
		{
			RemoveVarietyPairFromVarieties(index);
			base.SetItem(index, item);
			AddVarietyPairToVarieties(item);
		}

		private void AddVarietyPairToVarieties(VarietyPair vp)
		{
			vp.Variety1.VarietyPairs.VarietyPairAdded(vp);
			vp.Variety2.VarietyPairs.VarietyPairAdded(vp);
		}

		private void RemoveVarietyPairFromVarieties(int index)
		{
			VarietyPair vp = this[index];
			vp.Variety1.VarietyPairs.VarietyPairRemoved(vp);
			vp.Variety2.VarietyPairs.VarietyPairRemoved(vp);
		}
	}
}
