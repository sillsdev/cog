using GalaSoft.MvvmLight;

namespace SIL.Cog.Applications.ViewModels
{
	public class ExportGlobalCorrespondencesChartViewModel : ViewModelBase
	{
		private SoundCorrespondenceType _correspondenceType;
		private int _frequencyFilter;

		public SoundCorrespondenceType CorrespondenceType
		{
			get { return _correspondenceType; }
			set { Set(() => CorrespondenceType, ref _correspondenceType, value); }
		}

		public int FrequencyFilter
		{
			get { return _frequencyFilter; }
			set { Set(() => FrequencyFilter, ref _frequencyFilter, value); }
		}
	}
}
