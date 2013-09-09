using GalaSoft.MvvmLight;

namespace SIL.Cog.Applications.ViewModels
{
	public class ExportGlobalCorrespondencesChartViewModel : ViewModelBase
	{
		private ViewModelSyllablePosition _syllablePosition;
		private int _frequencyThreshold;

		public ViewModelSyllablePosition SyllablePosition
		{
			get { return _syllablePosition; }
			set { Set(() => SyllablePosition, ref _syllablePosition, value); }
		}

		public int FrequencyThreshold
		{
			get { return _frequencyThreshold; }
			set { Set(() => FrequencyThreshold, ref _frequencyThreshold, value); }
		}
	}
}
