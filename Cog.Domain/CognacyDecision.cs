using System;

namespace SIL.Cog.Domain
{
	public class CognacyDecision : IEquatable<CognacyDecision>
	{
		private readonly Variety _variety1;
		private readonly Variety _variety2;
		private readonly Meaning _meaning;
		private readonly bool _cognacy;

		public CognacyDecision(Variety variety1, Variety variety2, Meaning meaning, bool cognacy)
		{
			_variety1 = variety1;
			_variety2 = variety2;
			_meaning = meaning;
			_cognacy = cognacy;
		}

		public Variety Variety1
		{
			get { return _variety1; }
		}

		public Variety Variety2
		{
			get { return _variety2; }
		}

		public Meaning Meaning
		{
			get { return _meaning; }
		}

		public bool Cognacy
		{
			get { return _cognacy; }
		}

		public override bool Equals(object obj)
		{
			if (!(obj is CognacyDecision))
				return false;
			return Equals((CognacyDecision) obj);
		}

		public bool Equals(CognacyDecision other)
		{
			return other != null && _variety1 == other._variety1 && _variety2 == other._variety2 && _meaning == other._meaning && _cognacy == other._cognacy;
		}

		public override int GetHashCode()
		{
			int code = 23;
			code += code * 31 + _variety1.GetHashCode();
			code += code * 31 + _variety2.GetHashCode();
			code += code * 31 + _meaning.GetHashCode();
			code += code * 31 + _cognacy.GetHashCode();
			return code;
		}
	}
}
