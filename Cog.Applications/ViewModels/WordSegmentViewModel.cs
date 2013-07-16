using GalaSoft.MvvmLight;
using SIL.Cog.Domain;
using SIL.Machine;

namespace SIL.Cog.Applications.ViewModels
{
	public class WordSegmentViewModel : ViewModelBase
	{
		private readonly string _originalStrRep;
		private readonly string _strRep;
		private bool _isSelected;

		public WordSegmentViewModel(ShapeNode node)
		{
			_originalStrRep = node.OriginalStrRep();
			_strRep = node.StrRep();
		}

		public WordSegmentViewModel()
		{
			_originalStrRep = "|";
			_strRep = "|";
		}

		public string OriginalStrRep
		{
			get { return _originalStrRep; }
		}

		public string StrRep
		{
			get { return _strRep; }
		}

		public bool IsBoundary
		{
			get { return _originalStrRep == "|"; }
		}

		public bool IsSelected
		{
			get { return _isSelected; }
			set { Set(() => IsSelected, ref _isSelected, value); }
		}

		public override string ToString()
		{
			return _originalStrRep;
		}
	}
}
