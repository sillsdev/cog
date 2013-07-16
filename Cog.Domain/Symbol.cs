using SIL.Machine.FeatureModel;

namespace SIL.Cog.Domain
{
	public class Symbol
	{
		private readonly string _strRep;
		private readonly FeatureStruct _fs;
		private readonly bool _overwrite;

		public Symbol(string strRep)
			: this(strRep, null)
		{
			
		}

		public Symbol(string strRep, FeatureStruct fs)
			: this(strRep, fs, false)
		{
			
		}

		public Symbol(string strRep, FeatureStruct fs, bool overwrite)
		{
			_strRep = strRep.ToLowerInvariant();
			_fs = fs;
			_overwrite = overwrite;
		}

		public string StrRep
		{
			get { return _strRep; }
		}

		public FeatureStruct FeatureStruct
		{
			get { return _fs; }
		}

		public bool Overwrite
		{
			get { return _overwrite; }
		}
	}
}
