using SIL.Collections;
using SIL.Machine;

namespace SIL.Cog.Domain
{
	public enum AffixType
	{
		Prefix,
		Suffix
	}

	public class Affix : ObservableObject
	{
		private readonly string _strRep;
		private readonly AffixType _type;
		private Shape _shape;
		private readonly string _category;
		private double _score;

		public Affix(string strRep, AffixType type, string category)
		{
			_strRep = strRep;
			_type = type;
			_category = category;
		}

		public Shape Shape
		{
			get { return _shape; }
			internal set { Set(() => Shape, ref _shape, value); }
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
			set { Set(() => Score, ref _score, value); }
		}

		public override string ToString()
		{
			string strRep = StrRep;
			return _type == AffixType.Prefix ? strRep + "-" : "-" + strRep;
		}
	}
}
