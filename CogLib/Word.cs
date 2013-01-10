using System.Linq;
using System.Text;
using SIL.Collections;
using SIL.Machine;

namespace SIL.Cog
{
	public class Word : NotifyPropertyChangedBase, IData<ShapeNode>, IDeepCloneable<Word>
	{
		private readonly string _strRep;
		private Shape _shape;
		private readonly Sense _sense;

		public Word(string strRep, Shape shape, Sense sense)
		{
			_strRep = strRep;
			_shape = shape;
			_sense = sense;
		}

		private Word(Word word)
		{
			_strRep = word._strRep;
			_shape = word._shape.DeepClone();
			_shape.Freeze();
			_sense = word._sense;
		}

		public string StrRep
		{
			get { return _strRep; }
		}

		public Shape Shape
		{
			get { return _shape; }
			set
			{
				_shape = value;
				OnPropertyChanged("Shape");
			}
		}

		public Sense Sense
		{
			get { return _sense; }
		}

		public Variety Variety { get; internal set; }

		public Annotation<ShapeNode> Prefix
		{
			get
			{
				return _shape.Annotations.SingleOrDefault(ann => ann.Type() == CogFeatureSystem.PrefixType);
			}
		}

		public Annotation<ShapeNode> Stem
		{
			get
			{
				return _shape.Annotations.SingleOrDefault(ann => ann.Type() == CogFeatureSystem.StemType);
			}
		}

		public Annotation<ShapeNode> Suffix
		{
			get
			{
				return _shape.Annotations.SingleOrDefault(ann => ann.Type() == CogFeatureSystem.SuffixType);
			}
		}

		public Span<ShapeNode> Span
		{
			get { return _shape.Span; }
		}

		public AnnotationList<ShapeNode> Annotations
		{
			get { return _shape.Annotations; }
		}

		public Word DeepClone()
		{
			return new Word(this);
		}

		public override string ToString()
		{
			if (_shape.Count == 0)
				return _strRep;

			var sb = new StringBuilder();

			Annotation<ShapeNode> prefixAnn = Prefix;
			if (prefixAnn != null)
			{
				sb.Append(prefixAnn.OriginalStrRep());
				sb.Append(" ");
			}
			sb.Append("|");
			bool first = true;
			foreach (ShapeNode node in _shape.GetNodes(Stem.Span))
			{
				if (!first)
					sb.Append(" ");
				sb.Append(node.OriginalStrRep());
				first = false;
			}
			sb.Append("|");

			Annotation<ShapeNode> suffixAnn = Suffix;
			if (suffixAnn != null)
			{
				sb.Append(" ");
				sb.Append(suffixAnn.OriginalStrRep());
			}

			return sb.ToString();
		}
	}
}
