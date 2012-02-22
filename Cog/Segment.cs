using SIL.Machine.FeatureModel;

namespace SIL.Cog
{
	public class Segment
	{
		private static readonly Segment NullSegment = new Segment(FeatureStruct.New().Symbol(CogFeatureSystem.NullType).Feature(CogFeatureSystem.StrRep).EqualTo("-").Value, 0);
		public static Segment Null
		{
			get { return NullSegment; }
		}

		private readonly double _probability;
		private readonly FeatureStruct _fs;

		public Segment(FeatureStruct fs, double probability)
		{
			_fs = fs;
			_probability = probability;
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

		public double Probability
		{
			get { return _probability; }
		}

		public override string ToString()
		{
			return StrRep;
		}
	}
}
