using GalaSoft.MvvmLight;
using SIL.Machine;

namespace SIL.Cog.ViewModels
{
	public class AlignedNodeViewModel : ViewModelBase
	{
		private readonly Annotation<ShapeNode> _ann1;
		private readonly Annotation<ShapeNode> _ann2;
		private readonly string _note;
		private bool _isSelected;

		public AlignedNodeViewModel(Annotation<ShapeNode> ann1, Annotation<ShapeNode> ann2)
			: this(ann1, ann2, null)
		{
		}

		public AlignedNodeViewModel(Annotation<ShapeNode> ann1, Annotation<ShapeNode> ann2, string note)
		{
			_ann1 = ann1;
			_ann2 = ann2;
			_note = note;
		}

		public string StrRep1
		{
			get { return GetString(_ann1); }
		}

		public string StrRep2
		{
			get { return GetString(_ann2); }
		}

		public string Note
		{
			get { return _note; }
		}

		public bool IsSelected
		{
			get { return _isSelected; }
			set { Set("IsSelected", ref _isSelected, value); }
		}

		public Annotation<ShapeNode> Annotation1
		{
			get { return _ann1; }
		}

		public Annotation<ShapeNode> Annotation2
		{
			get { return _ann2; }
		}

		private static string GetString(Annotation<ShapeNode> ann)
		{
			if (ann == null)
				return "";

			return ann.StrRep();
		}
	}
}
