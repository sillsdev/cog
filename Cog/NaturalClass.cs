using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog
{
	public class NaturalClass : IDBearerBase
	{
		private readonly FeatureStruct _fs;

		public NaturalClass(string id, FeatureStruct fs)
			: base(id)
		{
			_fs = fs;
		}

		public FeatureSymbol Type
		{
			get { return (FeatureSymbol) _fs.GetValue(CogFeatureSystem.Type); }
		}

		public FeatureStruct FeatureStruct
		{
			get { return _fs; }
		}
	}
}
