using GalaSoft.MvvmLight;

namespace SIL.Cog.ViewModels
{
	public abstract class GlobalSegmentViewModel : ViewModelBase
	{
		private readonly string _strRep;

		protected GlobalSegmentViewModel(string strRep)
		{
			_strRep = strRep;
		}

		public string StrRep
		{
			get { return _strRep; }
		}
	}
}
