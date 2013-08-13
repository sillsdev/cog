using GalaSoft.MvvmLight;

namespace SIL.Cog.Applications.ViewModels
{
	public class ExportGlobalCorrespondencesChartViewModel : ViewModelBase
	{
		private ViewModelSyllablePosition _syllablePosition;
		private int _frequencyFilter;

		public ViewModelSyllablePosition SyllablePosition
		{
			get { return _syllablePosition; }
			set { Set(() => SyllablePosition, ref _syllablePosition, value); }
		}

		public int FrequencyFilter
		{
			get { return _frequencyFilter; }
			set { Set(() => FrequencyFilter, ref _frequencyFilter, value); }
		}
	}
}
