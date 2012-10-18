using SIL.Collections;
using SIL.Machine.FeatureModel;

namespace SIL.Cog
{
	public class Segment : NotifyPropertyChangedBase
	{
		public static readonly Segment Null = new Segment(FeatureStruct.New().Symbol(CogFeatureSystem.NullType).Feature(CogFeatureSystem.StrRep).EqualTo("-").Value);

		private readonly FeatureStruct _fs;
		private double _probability;
		private int _frequency;

		public Segment(FeatureStruct fs)
		{
			_fs = fs;
		}

		public string NormalizedStrRep
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
			internal set
			{
				_probability = value;
				OnPropertyChanged("Probability");
			}
		}

		public int Frequency
		{
			get { return _frequency; }
			internal set
			{
				_frequency = value;
				OnPropertyChanged("Frequency");
			}
		}

		public override string ToString()
		{
			return NormalizedStrRep;
		}
	}
}
