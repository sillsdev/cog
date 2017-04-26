using System;

namespace SIL.Cog.Domain
{
	public class CognacyDecision : IEquatable<CognacyDecision>
	{
		public CognacyDecision(Variety variety1, Variety variety2, Meaning meaning, bool cognacy)
		{
			Variety1 = variety1;
			Variety2 = variety2;
			Meaning = meaning;
			Cognacy = cognacy;
		}

		public Variety Variety1 { get; }

		public Variety Variety2 { get; }

		public Meaning Meaning { get; }

		public bool Cognacy { get; }

		public override bool Equals(object obj)
		{
			if (!(obj is CognacyDecision))
				return false;
			return Equals((CognacyDecision) obj);
		}

		public bool Equals(CognacyDecision other)
		{
			return other != null && Variety1 == other.Variety1 && Variety2 == other.Variety2 && Meaning == other.Meaning && Cognacy == other.Cognacy;
		}

		public override int GetHashCode()
		{
			int code = 23;
			code += code * 31 + Variety1.GetHashCode();
			code += code * 31 + Variety2.GetHashCode();
			code += code * 31 + Meaning.GetHashCode();
			code += code * 31 + Cognacy.GetHashCode();
			return code;
		}
	}
}
