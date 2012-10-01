using System.Collections.ObjectModel;

namespace SIL.Cog
{
	public class WordPairCollection : ObservableCollection<WordPair>
	{
		private readonly VarietyPair _varietyPair;
 
		internal WordPairCollection(VarietyPair varietyPair)
		{
			_varietyPair = varietyPair;
		}

		public WordPair Add(Word word1, Word word2)
		{
			var wordPair = new WordPair(_varietyPair, word1, word2);
			Add(wordPair);
			return wordPair;
		}
	}
}
