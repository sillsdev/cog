using SIL.Collections;

namespace SIL.Cog.Domain
{
	internal class MeaningCollection : KeyedBulkObservableList<string, Meaning>
	{
		protected override void InsertItem(int index, Meaning item)
		{
			base.InsertItem(index, item);
			item.Collection = this;
		}

		protected override string GetKeyForItem(Meaning item)
		{
			return item.Gloss;
		}

		protected override void SetItem(int index, Meaning item)
		{
			Meaning oldItem = Items[index];
			base.SetItem(index, item);
			item.Collection = this;
			oldItem.Collection = null;
		}

		protected override void RemoveItem(int index)
		{
			Meaning oldItem = Items[index];
			base.RemoveItem(index);
			oldItem.Collection = null;
		}

		protected override void ClearItems()
		{
			foreach (Meaning item in Items)
				item.Collection = null;

			base.ClearItems();
		}

		internal void ChangeMeaningGloss(Meaning meaning, string newGloss)
		{
			ChangeItemKey(meaning, newGloss);
		}
	}
}
