using GalaSoft.MvvmLight;
using SIL.Cog.Domain;
using SIL.Machine.Annotations;
using SIL.Machine.SequenceAlignment;

namespace SIL.Cog.Applications.ViewModels
{
	public class AlignedNodeViewModel : ViewModelBase
	{
		private readonly int _column;
		private readonly AlignmentCell<ShapeNode> _cell1;
		private readonly AlignmentCell<ShapeNode> _cell2;
		private readonly string _note;
		private bool _isSelected;

		public AlignedNodeViewModel(AlignmentCell<ShapeNode> cell1, AlignmentCell<ShapeNode> cell2)
			: this(-1, cell1, cell2, null)
		{
		}

		public AlignedNodeViewModel(int column, AlignmentCell<ShapeNode> cell1, AlignmentCell<ShapeNode> cell2, string note)
		{
			_column = column;
			_cell1 = cell1;
			_cell2 = cell2;
			_note = note;
		}

		public string StrRep1
		{
			get
			{
				if (_cell1.IsNull && _column != -1)
					return "-";
				return _cell1.StrRep();
			}
		}

		public string StrRep2
		{
			get
			{
				if (_cell2.IsNull && _column != -1)
					return "-";
				return _cell2.StrRep();
			}
		}

		public string Note
		{
			get { return _note; }
		}

		public bool IsSelected
		{
			get { return _isSelected; }
			set { Set(() => IsSelected, ref _isSelected, value); }
		}

		public int Column
		{
			get { return _column; }
		}

		internal AlignmentCell<ShapeNode> DomainCell1
		{
			get { return _cell1; }
		}

		internal AlignmentCell<ShapeNode> DomainCell2
		{
			get { return _cell2; }
		}
	}
}
