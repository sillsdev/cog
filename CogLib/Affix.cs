using SIL.Collections;
using SIL.Machine;

namespace SIL.Cog
{
	public enum AffixType
	{
		Prefix,
		Suffix
	}

	public class Affix : NotifyPropertyChangedBase
	{
		private readonly string _strRep;
		private readonly AffixType _type;
		private Shape _shape;
		private readonly string _category;
		private double _score;

		public Affix(string strRep, AffixType type, Shape shape, string category)
		{
			_strRep = strRep;
			_shape = shape;
			_type = type;
			_category = category;
		}

		public Shape Shape
		{
			get { return _shape; }
			set
			{
				_shape = value;
				OnPropertyChanged("Shape");
			}
		}

		public AffixType Type
		{
			get { return _type; }
		}

		public string Category
		{
			get { return _category; }
		}

		public string StrRep
		{
			get { return _strRep; }
		}

		public double Score
		{
			get { return _score; }
			set
			{
				_score = value;
				OnPropertyChanged("Score");
			}
		}

		public override string ToString()
		{
			string strRep = StrRep;
			return _type == AffixType.Prefix ? strRep + "-" : "-" + strRep;
		}
	}
}
