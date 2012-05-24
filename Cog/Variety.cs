using System.Collections.Generic;
using SIL.Collections;

namespace SIL.Cog
{
	public class Variety : IDBearerBase
	{
		private readonly WordCollection _words;
		private readonly SegmentCollection _segments;
		private readonly VarietyPairCollection _varietyPairs;
		private readonly List<Affix> _affixes;  

		public Variety(string id, IEnumerable<Word> words)
			: base(id)
		{
			_words = new WordCollection(words);
			_segments = new SegmentCollection(this);
			_varietyPairs = new VarietyPairCollection(this);
			_affixes = new List<Affix>();
		}

		public SegmentCollection Segments
		{
			get { return _segments; }
		}

		public WordCollection Words
		{
			get { return _words; }
		}

		public VarietyPairCollection VarietyPairs
		{
			get { return _varietyPairs; }
		}

		public ICollection<Affix> Affixes
		{
			get { return _affixes; }
		}
	}
}
