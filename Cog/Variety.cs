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
		private readonly Dictionary<string, Segment> _segments;
		private readonly List<VarietyPair> _varietyPairs;
		private readonly List<Affix> _affixes;  

		public Variety(string id, IEnumerable<Word> words)
			: base(id)
		{
			_words = new Dictionary<string, Word>();

			int total = 0;
			var segFreqs = new Dictionary<string, Tuple<FeatureStruct, int>>();
			foreach (Word word in words)
			{
				foreach (ShapeNode node in word.Shape)
				{
					segFreqs.UpdateValue(node.StrRep(), () => Tuple.Create(node.Annotation.FeatureStruct.Clone(), 0), tuple => Tuple.Create(tuple.Item1, tuple.Item2 + 1));
					total++;
				}
				_words[word.Gloss] = word;
			}

			_segments = segFreqs.ToDictionary(kvp => kvp.Key, kvp => new Segment(kvp.Value.Item1, (double) kvp.Value.Item2 / total));
			_varietyPairs = new List<VarietyPair>();
			_affixes = new List<Affix>();
		}

		public IReadOnlyCollection<Segment> Segments
		{
			get { return _segments.Values.AsReadOnlyCollection(); }
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

		public Segment GetSegment(string strRep)
		{
			return _segments[strRep];
		}

		public Segment GetSegment(ShapeNode node)
		{
			return _segments[node.StrRep()];
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
