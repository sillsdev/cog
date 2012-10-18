using GalaSoft.MvvmLight;

namespace SIL.Cog.ViewModels
{
	public class SoundCorrespondenceViewModel : ViewModelBase
	{
		private readonly SoundChange _soundChange;
		private readonly NSegment _correspondence;
		private readonly SoundCorrespondenceLhsViewModel _lhs;

		public SoundCorrespondenceViewModel(SoundChange soundChange, NSegment correspondence)
		{
			_soundChange = soundChange;
			_correspondence = correspondence;
			_lhs = new SoundCorrespondenceLhsViewModel(soundChange.Lhs);
		}

		public SoundCorrespondenceLhsViewModel Lhs
		{
			get { return _lhs; }
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

		public NSegment ModelCorrespondence
		{
			get { return _correspondence; }
		}
	}
}
