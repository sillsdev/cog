using System.Linq;
using System.Text;
using SIL.Collections;
using SIL.Machine;

namespace SIL.Cog.Domain
{
	public class Word : ObservableObject, IData<ShapeNode>, IDeepCloneable<Word>
	{
		private int _stemIndex;
		private int _stemLength;
		private readonly string _strRep;
		private Shape _shape;
		private readonly Sense _sense;

		public Word(string strRep, Sense sense)
			: this(strRep, 0, strRep.Length, sense)
		{
		}

		public Word(string strRep, int stemIndex, int stemLength, Sense sense)
		{
			_strRep = strRep;
			_sense = sense;
			_stemIndex = stemIndex;
			_stemLength = stemLength;
		}

		private Word(Word word)
		{
			_strRep = word._strRep;
			_stemIndex = word._stemIndex;
			_stemLength = word._stemLength;
			_shape = word._shape.DeepClone();
			_shape.Freeze();
			_sense = word._sense;
		}

		public string StrRep
		{
			get { return _strRep; }
		}

		public int StemIndex
		{
			get { return _stemIndex; }
			set { Set(() => StemIndex, ref _stemIndex, value); }
		}

		public int StemLength
		{
			get { return _stemLength; }
			set { Set(() => StemLength, ref _stemLength, value); }
		}

		public Shape Shape
		{
			get { return _shape; }
			internal set { Set(() => Shape, ref _shape, value); }
		}

		public bool IsValid
		{
			get { return _shape != null && _shape.Count > 0; }
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
