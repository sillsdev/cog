using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SIL.Collections;

namespace SIL.Cog
{
	public class Ngram : IReadOnlyList<Segment>, IEquatable<Ngram>
	{
		public static implicit operator Ngram(Segment seg)
		{
			if (seg == null)
				return new Ngram();

			return new Ngram(seg);
		}

		private readonly Segment[] _segments; 

		public Ngram(params Segment[] segs)
		{
			_segments = segs.ToArray();
		}

		public Ngram(IEnumerable<Segment> segs)
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

		public Segment First
		{
			get
			{
				if (_segments.Length == 0)
					throw new InvalidOperationException("The n-gram is empty.");
				return _segments[0];
			}
		}

		public Segment GetFirst(Direction dir)
		{
			if (_segments.Length == 0)
				throw new InvalidOperationException("The n-gram is empty.");
			return dir == Direction.LeftToRight ? First : Last;
		}

		public Segment Last
		{
			get
			{
				if (_segments.Length == 0)
					throw new InvalidOperationException("The n-gram is empty.");
				return _segments[_segments.Length - 1];
			}
		}

		public Segment GetLast(Direction dir)
		{
			if (_segments.Length == 0)
				throw new InvalidOperationException("The n-gram is empty.");

			return dir == Direction.LeftToRight ? Last : First;
		}

		public Ngram TakeAllExceptLast()
		{
			return TakeAllExceptLast(Direction.LeftToRight);
		}

		public Ngram TakeAllExceptLast(Direction dir)
		{
			return new Ngram(dir == Direction.LeftToRight ? _segments.Take(_segments.Length - 1) : _segments.Skip(1));
		}

		public Ngram SkipFirst()
		{
			return SkipFirst(Direction.LeftToRight);
		}

		public Ngram SkipFirst(Direction dir)
		{
			return new Ngram(dir == Direction.LeftToRight ? _segments.Skip(1) : _segments.Take(_segments.Length - 1));
		}

		public Ngram Concat(Segment seg)
		{
			return Concat(seg, Direction.LeftToRight);
		}

		public Ngram Concat(Segment seg, Direction dir)
		{
			return new Ngram(dir == Direction.LeftToRight ? _segments.Concat(seg) : seg.ToEnumerable().Concat(_segments));
		}

		public Ngram Concat(Ngram ngram)
		{
			return Concat(ngram, Direction.LeftToRight);
		}

		public Ngram Concat(Ngram ngram, Direction dir)
		{
			return new Ngram(dir == Direction.LeftToRight ? _segments.Concat(ngram) : ngram.Concat(_segments));
		}

		public bool Equals(Ngram other)
		{
			return other != null && _segments.SequenceEqual(other._segments);
		}

		public override bool Equals(object obj)
		{
			var nsegment = obj as Ngram;
			return obj != null && Equals(nsegment);
		}

		public override int GetHashCode()
		{
			return _segments.GetSequenceHashCode();
		}

		public override string ToString()
		{
			if (_segments.Length == 0)
				return "-";

			return string.Concat(_segments.Select(seg => seg.StrRep));
		}
	}
}
