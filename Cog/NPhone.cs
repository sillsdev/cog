using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SIL.Machine;

namespace SIL.Cog
{
	public class NPhone : IReadOnlyList<Phoneme>, IEquatable<NPhone>
	{
		private readonly Phoneme[] _phonemes; 

		public NPhone(params Phoneme[] phs)
		{
			_phonemes = phs.ToArray();
		}

		public NPhone(IEnumerable<Phoneme> phs)
		{
			_phonemes = phs.ToArray();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _phonemes.GetEnumerator();
		}

		IEnumerator<Phoneme> IEnumerable<Phoneme>.GetEnumerator()
		{
			return ((IEnumerable<Phoneme>) _phonemes).GetEnumerator();
		}

		public int Count
		{
			get { return _phonemes.Length; }
		}

		public Phoneme this[int index]
		{
			get { return _phonemes[index]; }
		}

		public bool Equals(NPhone other)
		{
			return other != null && _phonemes.SequenceEqual(other._phonemes);
		}

		public override bool Equals(object obj)
		{
			var nphone = obj as NPhone;
			return obj != null && Equals(nphone);
		}

		public override int GetHashCode()
		{
			return _phonemes.Aggregate(23, (code, ph) => code * 31 + ph.GetHashCode());
		}

		public override string ToString()
		{
			return string.Concat(_phonemes.Select(ph => ph.StrRep));
		}
	}
}
