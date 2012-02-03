using System;
using System.Collections.Generic;
using SIL.Machine;

namespace SIL.Cog
{
	public class VarietyPair
	{
		private readonly Variety _variety1;
		private readonly Variety _variety2;
		private readonly List<WordPair> _wordPairs; 
		private readonly Dictionary<Tuple<NaturalClass, NPhone, NaturalClass>, SoundChange> _soundChanges;
		private readonly double _defaultCorrespondenceProbability;
		private readonly int _possibleCorrespondenceCount;
		private readonly Dictionary<Phoneme, HashSet<Phoneme>> _similarPhonemes; 

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
			_soundChanges = new Dictionary<Tuple<NaturalClass, NPhone, NaturalClass>, SoundChange>();

			int phonemeCount = _variety2.Phonemes.Count;
			_possibleCorrespondenceCount = (phonemeCount * phonemeCount) + phonemeCount;
			_defaultCorrespondenceProbability = 1.0 / _possibleCorrespondenceCount;
			_similarPhonemes = new Dictionary<Phoneme, HashSet<Phoneme>>();
		}

		public Variety Variety1
		{
			get { return _variety1; }
		}

		public Variety Variety2
		{
			get { return _variety2; }
		}

		public IReadOnlyCollection<WordPair> WordPairs
		{
			get { return _wordPairs.AsReadOnlyCollection(); }
		}

		public double PhoneticSimilarityScore { get; set; }

		public double LexicalSimilarityScore { get; set; }

		public double Significance { get; set; }

		public double Precision { get; set; }

		public double Recall { get; set; }

		public double DefaultCorrespondenceProbability
		{
			get { return _defaultCorrespondenceProbability; }
		}

		public int PossibleCorrespondenceCount
		{
			get { return _possibleCorrespondenceCount; }
		}

		public IReadOnlyCollection<SoundChange> SoundChanges
		{
			get { return _soundChanges.Values.AsReadOnlyCollection(); }
		}

		public SoundChange GetSoundChange(NaturalClass leftEnv, NPhone target, NaturalClass rightEnv)
		{
			Tuple<NaturalClass, NPhone, NaturalClass> key = Tuple.Create(leftEnv, target, rightEnv);
			return _soundChanges.GetValue(key, () => new SoundChange(_possibleCorrespondenceCount, leftEnv, target, rightEnv));
		}

		public bool TryGetSoundChange(NaturalClass leftEnv, NPhone target, NaturalClass rightEnv, out SoundChange soundChange)
		{
			Tuple<NaturalClass, NPhone, NaturalClass> key = Tuple.Create(leftEnv, target, rightEnv);
			return _soundChanges.TryGetValue(key, out soundChange);
		}

		public void AddSimilarPhoneme(Phoneme ph1, Phoneme ph2)
		{
			HashSet<Phoneme> phonemes = _similarPhonemes.GetValue(ph1, () => new HashSet<Phoneme>());
			phonemes.Add(ph2);
		}

		public IReadOnlySet<Phoneme> GetSimilarPhonemes(Phoneme ph)
		{
			HashSet<Phoneme> phonemes;
			if (_similarPhonemes.TryGetValue(ph, out  phonemes))
				return phonemes.AsReadOnlySet();

			return new ReadOnlySet<Phoneme>(new HashSet<Phoneme>());
		}
	}
}
