using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight;
using SIL.Cog.Domain;
using SIL.Collections;
using SIL.Machine;

namespace SIL.Cog.Applications.ViewModels
{
	public class MultipleWordAlignmentWordViewModel : ViewModelBase
	{
		private readonly VarietyViewModel _variety;
		private readonly string _prefix;
		private readonly ReadOnlyList<string> _columns;
		private readonly string _suffix;
		private readonly int _cognateSetIndex;
		private readonly Word _word;

		public MultipleWordAlignmentWordViewModel(Word word, AlignmentCell<ShapeNode> prefix, IEnumerable<AlignmentCell<ShapeNode>> columns, AlignmentCell<ShapeNode> suffix, int cognateSetIndex)
		{
			_word = word;
			_variety = new VarietyViewModel(word.Variety);
			_prefix = prefix.StrRep();
			_columns = new ReadOnlyList<string>(columns.Select(cell => cell.IsNull ? "-" : cell.StrRep()).ToArray());
			_suffix = suffix.StrRep();
			_cognateSetIndex = cognateSetIndex;
		}

		public string StrRep
		{
			get { return _word.StrRep; }
		}

		public int CognateSetIndex
		{
			get { return _cognateSetIndex; }
		}

		public VarietyViewModel Variety
		{
			get { return _variety; }
		}

		public string Prefix
		{
			get { return _prefix; }
		}

		public ReadOnlyList<string> Columns
		{
			get { return _columns; }
		}

		public string Suffix
		{
			get { return _suffix; }
		}
	}
}
