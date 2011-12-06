using System;
using System.Collections.Generic;

namespace SIL.Cog
{
	public class WordIndex
	{
		private readonly Dictionary<string, HashSet<Word>> _glossIndex;
		private readonly Dictionary<string, HashSet<Word>> _languageIndex;
		private readonly Dictionary<Tuple<string, string>, Word> _glossLanguageIndex; 

		public WordIndex()
		{
			_glossIndex = new Dictionary<string, HashSet<Word>>();
			_languageIndex = new Dictionary<string, HashSet<Word>>();
			_glossLanguageIndex = new Dictionary<Tuple<string, string>, Word>();
		}

		public IEnumerable<string> Glosses
		{
			get { return _glossIndex.Keys; }
		}

		public int GlossCount
		{
			get { return _glossIndex.Count; }
		}

		public IEnumerable<string> Languages
		{
			get { return _languageIndex.Keys; }
		}

		public int LanguageCount
		{
			get { return _languageIndex.Count; }
		}

		public void Add(Word word)
		{
			Add(_glossIndex, word.Gloss, word);
			Add(_languageIndex, word.Language, word);
			_glossLanguageIndex[Tuple.Create(word.Gloss, word.Language)] = word;
		}

		private static void Add(Dictionary<string, HashSet<Word>> index, string key, Word word)
		{
			HashSet<Word> words;
			if (!index.TryGetValue(key, out words))
			{
				words = new HashSet<Word>();
				index[key] = words;
			}
			words.Add(word);
		}

		public bool TryGetWord(string gloss, string language, out Word word)
		{
			return _glossLanguageIndex.TryGetValue(Tuple.Create(gloss, language), out word);
		}

		public IEnumerable<Word> GetLanguageWords(string language)
		{
			return _languageIndex[language];
		}

		public IEnumerable<Word> GetGlossWords(string gloss)
		{
			return _glossIndex[gloss];
		}
	}
}
