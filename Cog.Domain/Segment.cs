using System;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Domain
{
	public class Segment : IEquatable<Segment>
	{
		public static readonly Segment Anchor = new Segment(FeatureStruct.New().Symbol(CogFeatureSystem.AnchorType).Feature(CogFeatureSystem.StrRep).EqualTo("#").Value);

		private readonly FeatureStruct _fs;

		public Segment(FeatureStruct fs)
		{
			if (!fs.IsFrozen)
				throw new ArgumentException("The feature structure must be immutable.", "fs");
			_fs = fs;
		}

		public string StrRep
		{
			get { return (string) _fs.GetValue(CogFeatureSystem.StrRep); }
		}

		public FeatureSymbol Type
		{
			get { return (FeatureSymbol) _fs.GetValue(CogFeatureSystem.Type); }
		}

		public FeatureStruct FeatureStruct
		{
			get { return _fs; }
		}

		public bool Equals(Segment other)
		{
			return other != null && StrRep == other.StrRep;
		}

		public override bool Equals(object obj)
		{
			var seg = obj as Segment;
			return seg != null && Equals(seg);
		}

		public override int GetHashCode()
		{
			return StrRep.GetHashCode();
		}

		public override string ToString()
		{
			return StrRep;
		}
	}
}
