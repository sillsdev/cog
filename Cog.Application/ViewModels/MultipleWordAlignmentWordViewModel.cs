using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight;
using SIL.Cog.Domain;
using SIL.Collections;
using SIL.Machine.Annotations;
using SIL.Machine.SequenceAlignment;

namespace SIL.Cog.Application.ViewModels
{
	public class MultipleWordAlignmentWordViewModel : ViewModelBase
	{
		private readonly MultipleWordAlignmentVarietyViewModel _variety;
		private readonly string _prefix;
		private readonly ReadOnlyList<string> _columns;
		private readonly string _suffix;
		private readonly int _cognateSetIndex;
		private readonly Word _word;
		private readonly MultipleWordAlignmentViewModel _parent;

		public MultipleWordAlignmentWordViewModel(MultipleWordAlignmentViewModel parent, Word word, AlignmentCell<ShapeNode> prefix, IEnumerable<AlignmentCell<ShapeNode>> columns, AlignmentCell<ShapeNode> suffix, int cognateSetIndex)
		{
			_word = word;
			ReadOnlyCollection<Word> words = word.Variety.Words[word.Meaning];
			_variety = new MultipleWordAlignmentVarietyViewModel(word.Variety, words.Count == 1 ? 0 : IndexOf(words, word));
			_prefix = prefix.StrRep();
			_columns = new ReadOnlyList<string>(columns.Select(cell => cell.IsNull ? "-" : cell.StrRep()).ToArray());
			_suffix = suffix.StrRep();
			_cognateSetIndex = cognateSetIndex;
			_parent = parent;
		}

		private static int IndexOf(IEnumerable<Word> words, Word word)
		{
			return words.Select((w, i) => new {Word = w, Index = i + 1}).First(wi => wi.Word == word).Index;
		}

		public string StrRep
		{
			get { return _word.StrRep; }
		}

		public int CognateSetIndex
		{
			get { return _cognateSetIndex; }
		}

		public MultipleWordAlignmentVarietyViewModel Variety
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

		public MultipleWordAlignmentViewModel Parent
		{
			get { return _parent; }
		}

		internal Word DomainWord
		{
			get { return _word; }
		}
	}
}
