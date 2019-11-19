using SIL.Machine.FeatureModel;
using SIL.ObjectModel;

namespace SIL.Cog.Domain
{
	public class SymbolCollection : KeyedBulkObservableList<string, Symbol>
	{
		public void Add(string strRep)
		{
			Add(new Symbol(strRep));
		}

		public void Add(string strRep, FeatureStruct fs)
		{
			Add(new Symbol(strRep, fs));
		}

		public void Add(string strRep, FeatureStruct fs, bool overwrite)
		{
			Add(new Symbol(strRep, fs, overwrite));
		}

		protected override string GetKeyForItem(Symbol item)
		{
			return item.StrRep;
		}
	}
}
