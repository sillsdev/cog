using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SIL.Collections;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog
{
	public class SegmentCollection : IReadOnlyCollection<Segment>
	{
		private readonly Dictionary<string, Segment> _segments;
		private readonly int _totalFreq;

		internal SegmentCollection(Variety variety)
		{
			_totalFreq = 0;
			var segFreqs = new Dictionary<string, Tuple<FeatureStruct, int>>();
			foreach (Sense sense in variety.Words.Senses)
			{
				foreach (Word word in variety.Words[sense])
				{
					foreach (ShapeNode node in word.Shape)
					{
						segFreqs.UpdateValue(node.StrRep(), () => Tuple.Create(node.Annotation.FeatureStruct.DeepClone(), 0), tuple => Tuple.Create(tuple.Item1, tuple.Item2 + 1));
						_totalFreq++;
					}
				}
			}

			_segments = segFreqs.ToDictionary(kvp => kvp.Key, kvp => new Segment(kvp.Value.Item1, (double) kvp.Value.Item2 / _totalFreq));
		}

		IEnumerator<Segment> IEnumerable<Segment>.GetEnumerator()
		{
			return _segments.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _segments.Values.GetEnumerator();
		}

		public bool Contains(Segment segment)
		{
			return _segments.ContainsKey(segment.StrRep);
		}

		public Segment this[string strRep]
		{
			get { return _segments[strRep]; }
		}

		public Segment this[ShapeNode node]
		{
			get { return _segments[node.StrRep()]; }
		}

		public int TotalFrequency
		{
			get { return _totalFreq; }
		}

		public int Count
		{
			get { return _segments.Count; }
		}
	}
}
