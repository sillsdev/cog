using SIL.ObjectModel;

namespace SIL.Cog.Domain
{
	internal class VarietyCollection : KeyedBulkObservableList<string, Variety>
	{
		protected override void InsertItem(int index, Variety item)
		{
			base.InsertItem(index, item);
			item.Collection = this;
		}

		protected override string GetKeyForItem(Variety item)
		{
			return item.Name;
		}

		protected override void SetItem(int index, Variety item)
		{
			Variety oldItem = Items[index];
			base.SetItem(index, item);
			item.Collection = this;
			oldItem.Collection = null;
		}

		protected override void RemoveItem(int index)
		{
			Variety oldItem = Items[index];
			base.RemoveItem(index);
			oldItem.Collection = null;
		}

		protected override void ClearItems()
		{
			foreach (Variety item in Items)
				item.Collection = null;

			base.ClearItems();
		}

		internal void ChangeVarietyName(Variety variety, string newName)
		{
			Dictionary.Remove(GetKeyForItem(variety));
			Dictionary.Add(newName, variety);
		}
	}
}
