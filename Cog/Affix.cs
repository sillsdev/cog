namespace SIL.Cog
{
	public enum AffixType
	{
		Prefix,
		Suffix
	}

	public class Affix
	{
		private readonly string _strRep;
		private readonly AffixType _type;
		private readonly string _category;

		public Affix(string strRep, AffixType type, string category)
		{
			_strRep = strRep;
			_type = type;
			_category = category;
		}

		public string StrRep
		{
			get { return _strRep; }
		}

		public AffixType Type
		{
			get { return _type; }
		}

		public string Category
		{
			get { return _category; }
		}

		public double Score { get; set; }

		public override string ToString()
		{
			return _type == AffixType.Prefix ? _strRep + "-" : "-" + _strRep;
		}
	}
}
