using GalaSoft.MvvmLight;
using SIL.Cog.Domain;
using SIL.Machine;

namespace SIL.Cog.Applications.ViewModels
{
	public class WordSegmentViewModel : ViewModelBase
	{
		private readonly string _strRep;
		private bool _isSelected;
		private readonly ShapeNode _node;
		private readonly bool _isNotInOriginal;

		public WordSegmentViewModel(ShapeNode node)
		{
			_node = node;
			_strRep = node.OriginalStrRep();
		}

		public WordSegmentViewModel(string strRep)
		{
			_strRep = strRep;
			_isNotInOriginal = true;
		}

		public string StrRep
		{
			get { return _strRep; }
		}

		public bool IsBoundary
		{
			get { return _strRep == "|"; }
		}

		public bool IsNotInOriginal
		{
			get { return _isNotInOriginal; }
		}

		public bool IsSelected
		{
			get { return _isSelected; }
			set { Set(() => IsSelected, ref _isSelected, value); }
		}

		internal ShapeNode DomainNode
		{
			get { return _node; }
		}

		public override string ToString()
		{
			return _strRep;
		}
	}
}
