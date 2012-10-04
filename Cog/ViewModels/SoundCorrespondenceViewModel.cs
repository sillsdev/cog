using GalaSoft.MvvmLight;

namespace SIL.Cog.ViewModels
{
	public class SoundCorrespondenceViewModel : ViewModelBase
	{
		private readonly SoundChange _soundChange;
		private readonly NSegment _correspondence;

		public SoundCorrespondenceViewModel(SoundChange soundChange, NSegment correspondence)
		{
			_soundChange = soundChange;
			_correspondence = correspondence;
		}

		public string Lhs
		{
			get { return _soundChange.Lhs.ToString(); }
		}

		public string Correspondence
		{
			get { return _correspondence.ToString(); }
		}

		public double Probability
		{
			get { return _soundChange[_correspondence]; }
		}

		public SoundChange ModelSoundChange
		{
			get { return _soundChange; }
		}
	}
}
