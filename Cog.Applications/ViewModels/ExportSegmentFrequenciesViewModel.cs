using GalaSoft.MvvmLight;

namespace SIL.Cog.Applications.ViewModels
{
	public class ExportSegmentFrequenciesViewModel : ViewModelBase
	{
		private ViewModelSyllablePosition _syllablePosition;

		public ViewModelSyllablePosition SyllablePosition
		{
			get { return _syllablePosition; }
			set { Set(() => SyllablePosition, ref _syllablePosition, value); }
		}
	}
}
