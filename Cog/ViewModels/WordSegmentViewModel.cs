using GalaSoft.MvvmLight;
using SIL.Machine;

namespace SIL.Cog.ViewModels
{
	public class WordSegmentViewModel : ViewModelBase
	{
		private readonly string _strRep;

		public WordSegmentViewModel(ShapeNode node)
		{
			_strRep = node.StrRep();
		}

		public WordSegmentViewModel(string strRep)
		{
			_strRep = strRep;
		}

		public string StrRep
		{
			get { return _strRep; }
		}

		public bool IsBoundary
		{
			get { return _strRep == "|"; }
		}

		public override string ToString()
		{
			return _strRep;
		}
	}
}
