using System.Collections.Generic;
using System.Linq;
using System.Text;
using SIL.Extensions;
using SIL.Machine.Annotations;
using SIL.ObjectModel;

namespace SIL.Cog.Domain
{
	public class Word : ObservableObject, IAnnotatedData<ShapeNode>, ICloneable<Word>
	{
		private int _stemIndex;
		private int _stemLength;
		private Shape _shape;

		public Word(string strRep, Meaning meaning)
			: this(strRep, 0, strRep.Length, meaning)
		{
		}

		public Word(string strRep, int stemIndex, int stemLength, Meaning meaning)
		{
			StrRep = strRep;
			Meaning = meaning;
			_stemIndex = stemIndex;
			_stemLength = stemLength;
		}

		public Word(Word word)
		{
			StrRep = word.StrRep;
			Meaning = word.Meaning;
			_stemIndex = word._stemIndex;
			_stemLength = word._stemLength;
			_shape = word._shape.Clone();
			Audio = word.Audio;
			Participants = word.Participants;
		}

		public string StrRep { get; }

		public int StemIndex
		{
			get => _stemIndex;
			set => Set(() => StemIndex, ref _stemIndex, value);
		}

		public int StemLength
		{
			get => _stemLength;
			set => Set(() => StemLength, ref _stemLength, value);
		}

		public Shape Shape
		{
			get => _shape;
			internal set => Set(() => Shape, ref _shape, value);
		}

		public bool IsValid => _shape != null && _shape.Count > 0;

		public Meaning Meaning { get; }

		public Variety Variety { get; internal set; }

		public Annotation<ShapeNode> Prefix => _shape.Annotations
			.SingleOrDefault(ann => ann.Type() == CogFeatureSystem.PrefixType);

		public Annotation<ShapeNode> Stem => _shape.Annotations
			.SingleOrDefault(ann => ann.Type() == CogFeatureSystem.StemType);

		public Annotation<ShapeNode> Suffix => _shape.Annotations
			.SingleOrDefault(ann => ann.Type() == CogFeatureSystem.SuffixType);

		public Range<ShapeNode> Range => _shape.Range;

		public AnnotationList<ShapeNode> Annotations => _shape.Annotations;

		public Audio Audio { get; set; }
		public string Participants { get; set; }

		public Word Clone()
		{
			return new Word(this);
		}

		public override string ToString()
		{
			if (_shape == null || _shape.Count == 0)
				return StrRep;

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
				foreach (ShapeNode node in _shape.GetNodes(child.Range))
				{
					string strRep = node.OriginalStrRep();
					if (string.IsNullOrEmpty(strRep))
						continue;

					yield return strRep;
				}
				if (child.Type() == CogFeatureSystem.SyllableType && child.Range.End != ann.Range.End
					&& !child.Range.End.Next.Type().IsOneOf(CogFeatureSystem.BoundaryType, CogFeatureSystem.ToneLetterType))
				{
					yield return ".";
				}
			}
		}
	}
}
