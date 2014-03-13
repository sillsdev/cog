using SIL.Collections;

namespace SIL.Cog.Domain
{
	public class WordPairCollection : KeyedBulkObservableList<Meaning, WordPair>
	{
		private readonly VarietyPair _varietyPair;
 
		internal WordPairCollection(VarietyPair varietyPair)
		{
			_varietyPair = varietyPair;
		}

		protected override Meaning GetKeyForItem(WordPair item)
		{
			return item.Meaning;
		}

		public WordPair Add(Word word1, Word word2)
		{
			var wordPair = new WordPair(word1, word2);
			Add(wordPair);
			return wordPair;
		}

		protected override void InsertItem(int index, WordPair item)
		{
			base.InsertItem(index, item);
			item.VarietyPair = _varietyPair;
		}

		protected override void RemoveItem(int index)
		{
			this[index].VarietyPair = null;
			base.RemoveItem(index);
		}

		protected override void ClearItems()
		{
			foreach (WordPair wp in this)
				wp.VarietyPair = null;
			base.ClearItems();
		}

		protected override void SetItem(int index, WordPair item)
		{
			this[index].VarietyPair = null;
			base.SetItem(index, item);
			item.VarietyPair = _varietyPair;
		}
	}
}
