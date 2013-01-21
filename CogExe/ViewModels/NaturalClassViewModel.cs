using GalaSoft.MvvmLight;

namespace SIL.Cog.ViewModels
{
	public class NaturalClassViewModel : ViewModelBase
	{
		private readonly NaturalClass _naturalClass;

		public NaturalClassViewModel(NaturalClass naturalClass)
		{
			_naturalClass = naturalClass;
		}

		public string Name
		{
			get { return _naturalClass.Name; }
		}

		public SoundType Type
		{
			get
			{
				if (_naturalClass.Type == CogFeatureSystem.ConsonantType)
					return SoundType.Consonant;
				return SoundType.Vowel;
			}
		}

		public string FeatureStructure
		{
			get { return ViewModelUtilities.GetFeatureStructureString(_naturalClass.FeatureStruct); }
		}

		public NaturalClass ModelNaturalClass
		{
			get { return _naturalClass; }
		}
	}
}
