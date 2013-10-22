using GalaSoft.MvvmLight;

namespace SIL.Cog.Applications.ViewModels
{
	public class ExportGlobalCorrespondencesChartViewModel : ViewModelBase
	{
		private SyllablePosition _syllablePosition;
		private int _frequencyThreshold;

		public SyllablePosition SyllablePosition
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
