using GalaSoft.MvvmLight;

namespace SIL.Cog.ViewModels
{
	public class SoundClassViewModel : ViewModelBase
	{
		private readonly SoundClass _soundClass;

		public SoundClassViewModel(SoundClass soundClass)
		{
			_soundClass = soundClass;
		}

		public string Name
		{
			get { return _soundClass.Name; }
		}

		public string Type
		{
			get
			{
				var nc = _soundClass as NaturalClass;
				if (nc != null)
					return nc.Type == CogFeatureSystem.ConsonantType ? "Consonant" : "Vowel";
				var unc = _soundClass as UnnaturalClass;
				if (unc != null)
					return "Unnatural";
				return "";
			}
		}

		public string Description
		{
			get
			{
				var nc = _soundClass as NaturalClass;
				if (nc != null)
					return nc.FeatureStruct.GetString();
				var unc = _soundClass as UnnaturalClass;
				if (unc != null)
					return string.Join(", ", unc.Segments);
				return "";
			}
		}

		public SoundClass ModelSoundClass
		{
			get { return _soundClass; }
		}
	}
}
