using System;
using System.Collections.Generic;
using System.Linq;

namespace SIL.Cog
{
	public class VarietyPair
	{
		private readonly Variety _variety1;
		private readonly Variety _variety2;
		private readonly List<WordPair> _wordPairs; 
		private readonly Dictionary<Tuple<NaturalClass, string, NaturalClass>, SoundChange> _soundChanges;

		public VarietyPair(Variety variety1, Variety variety2)
		{
			_variety1 = variety1;
			_variety2 = variety2;
			_wordPairs = new List<WordPair>();
			foreach (Word word1 in _variety1.Words)
			{
				Word word2;
				if (_variety2.TryGetWord(word1.Gloss, out word2))
					_wordPairs.Add(new WordPair(this, word1, word2));
			}
			_soundChanges = new Dictionary<Tuple<NaturalClass, string, NaturalClass>, SoundChange>();
		}

		public Variety Variety1
		{
			get { return _variety1; }
		}

		public Variety Variety2
		{
			get { return _variety2; }
		}

		public IEnumerable<WordPair> WordPairs
		{
			get { return _wordPairs; }
		}

		public int WordPairCount
		{
			get { return _wordPairs.Count; }
		}

		public IEnumerable<WordPair> Cognates
		{
			get { return _wordPairs.Where(wordPair => wordPair.AreCognates); }
		}

		public double PhoneticSimilarityScore { get; set; }

		public double LexicalSimilarityScore { get; set; }

		public IEnumerable<SoundChange> SoundChanges
		{
			get { return _soundChanges.Values; }
		}

		public SoundChange AddSoundChange(NaturalClass leftEnv, string target, NaturalClass rightEnv)
		{
			Tuple<NaturalClass, string, NaturalClass> key = Tuple.Create(leftEnv, target, rightEnv);
			var soundChange = new SoundChange(_variety2.PhonemeCount, leftEnv, target, rightEnv);
			_soundChanges[key] = soundChange;
			return soundChange;
		}

		public bool TryGetSoundChange(NaturalClass leftEnv, string target, NaturalClass rightEnv, out SoundChange soundChange)
		{
			Tuple<NaturalClass, string, NaturalClass> key = Tuple.Create(leftEnv, target, rightEnv);
			return _soundChanges.TryGetValue(key, out soundChange);
		}

		public double GetCorrespondenceProbability(NaturalClass leftEnv, string target, NaturalClass rightEnv, string correspondence)
		{
			SoundChange change;
			if (TryGetSoundChange(leftEnv, target, rightEnv, out change))
				return change[correspondence];

			return 1.0 / ((_variety2.PhonemeCount * _variety2.PhonemeCount) + _variety2.PhonemeCount);
		}
	}
}
