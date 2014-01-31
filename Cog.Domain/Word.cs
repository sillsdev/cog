using System.Collections.Generic;
using System.Linq;
using System.Text;
using SIL.Collections;
using SIL.Machine.Annotations;

namespace SIL.Cog.Domain
{
	public class Word : ObservableObject, IAnnotatedData<ShapeNode>
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

		public override string ToString()
		{
			if (_shape.Count == 0)
				return _strRep;

			var sb = new StringBuilder();

			Annotation<ShapeNode> prefixAnn = Prefix;
			if (prefixAnn != null)
			{
				sb.Append(string.Concat(GetOriginalStrRep(prefixAnn)));
				sb.Append(" ");
			}
			sb.Append("|");
			sb.Append(string.Join(" ", GetOriginalStrRep(Stem)));
			sb.Append("|");

			Annotation<ShapeNode> suffixAnn = Suffix;
			if (suffixAnn != null)
			{
				sb.Append(" ");
				sb.Append(string.Concat(GetOriginalStrRep(suffixAnn)));
			}

			return sb.ToString();
		}

		private IEnumerable<string> GetOriginalStrRep(Annotation<ShapeNode> ann)
		{
			foreach (Annotation<ShapeNode> child in ann.Children)
			{
				foreach (ShapeNode node in _shape.GetNodes(child.Span))
				{
					string strRep = node.OriginalStrRep();
					if (string.IsNullOrEmpty(strRep))
						continue;

					yield return strRep;
				}
				if (child.Type() == CogFeatureSystem.SyllableType && child.Span.End != ann.Span.End
					&& !child.Span.End.Next.Type().IsOneOf(CogFeatureSystem.BoundaryType, CogFeatureSystem.ToneLetterType))
				{
					yield return ".";
				}
			}
		}
	}
}
