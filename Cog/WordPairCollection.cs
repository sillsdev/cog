using System.Collections;
using System.Collections.Generic;

namespace SIL.Cog
{
	public class WordPairCollection : ICollection<WordPair>
	{
		private readonly HashSet<WordPair> _wordPairs;
		private readonly VarietyPair _varietyPair;
 
		internal WordPairCollection(VarietyPair varietyPair)
		{
			_wordPairs = new HashSet<WordPair>();
			_varietyPair = varietyPair;
		}

		IEnumerator<WordPair> IEnumerable<WordPair>.GetEnumerator()
		{
			return _wordPairs.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _wordPairs.GetEnumerator();
		}

		void ICollection<WordPair>.Add(WordPair item)
		{
			_wordPairs.Add(item);
		}

		public WordPair Add(Word word1, Word word2)
		{
			var wordPair = new WordPair(_varietyPair, word1, word2);
			_wordPairs.Add(wordPair);
			return wordPair;
		}

		public void Clear()
		{
			_wordPairs.Clear();
		}

		public bool Contains(WordPair item)
		{
			return _wordPairs.Contains(item);
		}

		public void CopyTo(WordPair[] array, int arrayIndex)
		{
			_wordPairs.CopyTo(array, arrayIndex);
		}

		public bool Remove(WordPair item)
		{
			return _wordPairs.Remove(item);
		}

		public int Count
		{
			get { return _wordPairs.Count; }
		}

		bool ICollection<WordPair>.IsReadOnly
		{
			get { return false; }
		}
	}
}
