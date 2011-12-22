using System.Collections.Generic;
using System.Linq;
using SIL.Machine;

namespace SIL.Cog
{
	public class Variety : IDBearerBase
	{
		private readonly Dictionary<string, Word> _words;
		private readonly HashSet<string> _phonemes;
		private readonly List<VarietyPair> _varietyPairs;

		public Variety(string id, IEnumerable<Word> words)
			: base(id)
		{
			_words = new Dictionary<string, Word>();
			foreach (Word word in words)
				_words[word.Gloss] = word;
			_phonemes = new HashSet<string>(_words.Values.SelectMany(word => word.Shape,
				(word, node) => (string) node.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep)));
			_varietyPairs = new List<VarietyPair>();
		}

		public int PhonemeCount
		{
			get
			{
				return _phonemes.Count;
			}
		}

		public IEnumerable<string> Phonemes
		{
			get { return _phonemes; }
		}

		public IEnumerable<Word> Words
		{
			get { return _words.Values; }
		}

		public IEnumerable<VarietyPair> VarietyPairs
		{
			get { return _varietyPairs; }
		}

		public Word GetWord(string glossID)
		{
			return _words[glossID];
		}

		public bool TryGetWord(string glossID, out Word word)
		{
			return _words.TryGetValue(glossID, out word);
		}

		public void AddVarietyPair(VarietyPair varietyPair)
		{
			_varietyPairs.Add(varietyPair);
		}
	}
}
