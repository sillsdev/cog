using System;
using System.Collections.Generic;
using System.Linq;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog
{
	public class Variety : IDBearerBase
	{
		private readonly Dictionary<Sense, List<Word>> _words;
		private readonly Dictionary<string, Segment> _segments;
		private readonly List<VarietyPair> _varietyPairs;
		private readonly List<Affix> _affixes;  

		public Variety(string id, IEnumerable<Word> words)
			: base(id)
		{
			_words = new Dictionary<Sense, List<Word>>();

			int total = 0;
			var segFreqs = new Dictionary<string, Tuple<FeatureStruct, int>>();
			foreach (Word word in words)
			{
				foreach (ShapeNode node in word.Shape)
				{
					segFreqs.UpdateValue(node.StrRep(), () => Tuple.Create(node.Annotation.FeatureStruct.Clone(), 0), tuple => Tuple.Create(tuple.Item1, tuple.Item2 + 1));
					total++;
				}
				List<Word> senseWords = _words.GetValue(word.Sense, () => new List<Word>());
				senseWords.Add(word);
			}

			_segments = segFreqs.ToDictionary(kvp => kvp.Key, kvp => new Segment(kvp.Value.Item1, (double) kvp.Value.Item2 / total));
			_varietyPairs = new List<VarietyPair>();
			_affixes = new List<Affix>();
		}

		public IReadOnlyCollection<Segment> Segments
		{
			get { return _segments.Values.AsReadOnlyCollection(); }
		}

		public IReadOnlyCollection<Sense> Senses
		{
			get { return _words.Keys.AsReadOnlyCollection(); }
		}

		public IReadOnlyCollection<VarietyPair> VarietyPairs
		{
			get { return _varietyPairs.AsReadOnlyCollection(); }
		}

		public IReadOnlyCollection<Affix> Affixes
		{
			get { return _affixes.AsReadOnlyCollection(); }
		}

		public IReadOnlyCollection<Word> GetWords(Sense sense)
		{
			List<Word> words;
			if (_words.TryGetValue(sense, out words))
				return words.AsReadOnlyCollection();
			return new ReadOnlyCollection<Word>(new Word[0]);
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
