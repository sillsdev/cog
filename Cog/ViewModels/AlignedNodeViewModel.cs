using GalaSoft.MvvmLight;

namespace SIL.Cog.ViewModels
{
	public class AlignedNodeViewModel : ViewModelBase
	{
		private readonly string _strRep1;
		private readonly string _strRep2;
		private readonly string _note;

		public AlignedNodeViewModel(string strRep1, string strRep2)
			: this(strRep1, strRep2, null)
		{
		}

		public AlignedNodeViewModel(string strRep1, string strRep2, string note)
		{
			_strRep1 = strRep1;
			_strRep2 = strRep2;
			_note = note;
		}

		public string StrRep1
		{
			get { return _strRep1; }
		}

		public string StrRep2
		{
			get { return _strRep2; }
		}

		public string Note
		{
			get { return _note; }
		}
	}
}
