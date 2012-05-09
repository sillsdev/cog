using System.Collections.Generic;
using SIL.Collections;

namespace SIL.Cog
{
	public class Variety : IDBearerBase
	{
		private readonly Dictionary<Sense, List<Word>> _words;
		private readonly SegmentCollection _segments;
		private readonly VarietyPairCollection _varietyPairs;
		private readonly List<Affix> _affixes;  

		public Variety(string id, IEnumerable<Word> words)
			: base(id)
		{
			_words = new Dictionary<Sense, List<Word>>();
			foreach (Word word in words)
			{
				List<Word> senseWords = _words.GetValue(word.Sense, () => new List<Word>());
				senseWords.Add(word);
			}

			_segments = new SegmentCollection(this);
			_varietyPairs = new VarietyPairCollection(this);
			_affixes = new List<Affix>();
		}

		public SegmentCollection Segments
		{
			get { return _segments; }
		}

		public IReadOnlyCollection<Sense> Senses
		{
			get { return _words.Keys.AsReadOnlyCollection(); }
		}

		public VarietyPairCollection VarietyPairs
		{
			get { return _varietyPairs; }
		}

		public ICollection<Affix> Affixes
		{
			get { return _affixes; }
		}

		public IReadOnlyCollection<Word> GetWords(Sense sense)
		{
			List<Word> words;
			if (_words.TryGetValue(sense, out words))
				return words.AsReadOnlyCollection();
			return new ReadOnlyCollection<Word>(new Word[0]);
		}
	}
}
