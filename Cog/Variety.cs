using System;
using System.Collections.Generic;
using System.Linq;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog
{
	public class Variety : IDBearerBase
	{
		private readonly Dictionary<string, Word> _words;
		private readonly Dictionary<string, Phoneme> _phonemes;
		private readonly List<VarietyPair> _varietyPairs;
		private readonly List<Affix> _affixes;  

		public Variety(string id, IEnumerable<Word> words)
			: base(id)
		{
			_words = new Dictionary<string, Word>();

			int total = 0;
			var phonFreqs = new Dictionary<string, Tuple<FeatureStruct, int>>();
			foreach (Word word in words)
			{
				foreach (ShapeNode node in word.Shape)
				{
					phonFreqs.UpdateValue(node.StrRep(), () => Tuple.Create(node.Annotation.FeatureStruct.Clone(), 0), tuple => Tuple.Create(tuple.Item1, tuple.Item2 + 1));
					total++;
				}
				_words[word.Gloss] = word;
			}

			_phonemes = phonFreqs.ToDictionary(kvp => kvp.Key, kvp => new Phoneme(kvp.Value.Item1, (double) kvp.Value.Item2 / total));
			_varietyPairs = new List<VarietyPair>();
			_affixes = new List<Affix>();
		}

		public IReadOnlyCollection<Phoneme> Phonemes
		{
			get { return _phonemes.Values.AsReadOnlyCollection(); }
		}

		public IReadOnlyCollection<Word> Words
		{
			get { return _words.Values.AsReadOnlyCollection(); }
		}

		public IReadOnlyCollection<VarietyPair> VarietyPairs
		{
			get { return _varietyPairs.AsReadOnlyCollection(); }
		}

		public IReadOnlyCollection<Affix> Affixes
		{
			get { return _affixes.AsReadOnlyCollection(); }
		}

		public Word GetWord(string glossID)
		{
			return _words[glossID];
		}

		public bool TryGetWord(string glossID, out Word word)
		{
			return _words.TryGetValue(glossID, out word);
		}

		public Phoneme GetPhoneme(string strRep)
		{
			return _phonemes[strRep];
		}

		public Phoneme GetPhoneme(ShapeNode node)
		{
			return _phonemes[node.StrRep()];
		}

		public void AddVarietyPair(VarietyPair varietyPair)
		{
			_varietyPairs.Add(varietyPair);
		}

		public void AddAffix(Affix affix)
		{
			_affixes.Add(affix);
		}
	}
}
