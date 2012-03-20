using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SIL.Collections;
using SIL.Machine;

namespace SIL.Cog
{
	public class NSegment : IReadOnlyList<Segment>, IEquatable<NSegment>
	{
		private readonly Segment[] _segments; 

		public NSegment(params Segment[] segs)
		{
			_segments = segs.ToArray();
		}

		public NSegment(IEnumerable<Segment> segs)
		{
			_segments = segs.ToArray();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _segments.GetEnumerator();
		}

		IEnumerator<Segment> IEnumerable<Segment>.GetEnumerator()
		{
			return ((IEnumerable<Segment>) _segments).GetEnumerator();
		}

		public int Count
		{
			get { return _segments.Length; }
		}

		public Segment this[int index]
		{
			get { return _segments[index]; }
		}

		public bool Equals(NSegment other)
		{
			return other != null && _segments.SequenceEqual(other._segments);
		}

		public override bool Equals(object obj)
		{
			var nphone = obj as NSegment;
			return obj != null && Equals(nphone);
		}

		public override int GetHashCode()
		{
			return _segments.Aggregate(23, (code, seg) => code * 31 + seg.GetHashCode());
		}

		public override string ToString()
		{
			return string.Concat(_segments.Select(seg => seg.StrRep));
		}
	}
}
