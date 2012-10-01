using SIL.Collections;
using SIL.Machine.FeatureModel;

namespace SIL.Cog
{
	public class NaturalClass : NotifyPropertyChangedBase
	{
		private readonly FeatureStruct _fs;
		private string _name;

		public NaturalClass(string name, FeatureStruct fs)
		{
			_name = name;
			_fs = fs;
		}

		public string Name
		{
			get { return _name; }
			set
			{
				_name = value;
				OnPropertyChanged("Name");
			}
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
