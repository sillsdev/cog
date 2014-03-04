using GalaSoft.MvvmLight;

namespace SIL.Cog.Application.ViewModels
{
	public class ExportSegmentFrequenciesViewModel : ViewModelBase
	{
		private SyllablePosition _syllablePosition;

		public SyllablePosition SyllablePosition
		{
			get { return _syllablePosition; }
			set { Set(() => SyllablePosition, ref _syllablePosition, value); }
		}
	}
}
