using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using SIL.Collections;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog
{
	public static class CogExtensions
	{
		public static string OriginalStrRep(this ShapeNode node)
		{
			return (string) node.Annotation.FeatureStruct.GetValue(CogFeatureSystem.OriginalStrRep);
		}

		public static string OriginalStrRep(this Annotation<ShapeNode> ann)
		{
			return ann.Span.Start.GetNodes(ann.Span.End).OriginalStrRep();
		}

		public static string OriginalStrRep(this IEnumerable<ShapeNode> nodes)
		{
			return string.Concat(nodes.Select(node => node.OriginalStrRep()));
		}

		public static string StrRep(this ShapeNode node)
		{
			return (string) node.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep);
		}

		public static string StrRep(this Annotation<ShapeNode> ann)
		{
			return ann.Span.Start.GetNodes(ann.Span.End).StrRep();
		}

		public static string StrRep(this IEnumerable<ShapeNode> nodes)
		{
			return string.Concat(nodes.Select(node => node.StrRep()));
		}

		public static FeatureSymbol Type(this ShapeNode node)
		{
			return (FeatureSymbol) node.Annotation.FeatureStruct.GetValue(CogFeatureSystem.Type);
		}

		public static FeatureSymbol Type(this Annotation<ShapeNode> ann)
		{
			return (FeatureSymbol) ann.FeatureStruct.GetValue(CogFeatureSystem.Type);
		}

		public static SoundContext ToSoundContext(this ShapeNode node, Variety variety, IEnumerable<SoundClass> soundClasses)
		{
			ShapeNode prevNode = node.GetPrev(NodeFilter);
			SoundClass leftEnv = prevNode.GetMatchingSoundClass(variety, soundClasses);
			Ngram target = node.ToNgram(variety);
			ShapeNode nextNode = node.GetNext(NodeFilter);
			SoundClass rightEnv = nextNode.GetMatchingSoundClass(variety, soundClasses);
			return new SoundContext(leftEnv, target, rightEnv);
		}

		public static SoundClass GetMatchingSoundClass(this ShapeNode node, Variety variety, IEnumerable<SoundClass> soundClasses)
		{
			Annotation<ShapeNode> stemAnn = ((Shape) node.List).Annotations.First(ann => ann.Type() == CogFeatureSystem.StemType);
			Segment left = null;
			if (stemAnn.Span.Contains(node) || node.Annotation.CompareTo(stemAnn) > 0)
			{
				ShapeNode leftNode = node.GetPrev(NodeFilter);
				if (leftNode != null)
					left = stemAnn.Span.Contains(leftNode) ? variety.Segments[leftNode] : Segment.Anchor;
			}

			Ngram target = stemAnn.Span.Contains(node) ? node.ToNgram(variety) : new Ngram(Segment.Anchor);

			Segment right = null;
			if (stemAnn.Span.Contains(node) || node.Annotation.CompareTo(stemAnn) < 0)
			{
				ShapeNode rightNode = node.GetNext(NodeFilter);
				if (rightNode != null)
					right = stemAnn.Span.Contains(rightNode) ? variety.Segments[rightNode] : Segment.Anchor;
			}

			return soundClasses.FirstOrDefault(sc => sc.Matches(left, target, right));
		}

		public static SoundClass GetMatchingSoundClass(this Alignment<ShapeNode> alignment, int seq, int col, Word word, IEnumerable<SoundClass> soundClasses)
		{
			ShapeNode leftNode = GetLeftNode(alignment, word, seq, col);
			Segment left = leftNode == null ? null : word.Variety.Segments[leftNode];
			Ngram target = alignment[seq, col].ToNgram(word.Variety);
			ShapeNode rightNode = GetRightNode(alignment, word, seq, col);
			Segment right = rightNode == null ? null : word.Variety.Segments[rightNode];
			return soundClasses.FirstOrDefault(sc => sc.Matches(left, target, right));
		}

		private static bool NodeFilter(ShapeNode node)
		{
			return node.Type().IsOneOf(CogFeatureSystem.VowelType, CogFeatureSystem.ConsonantType, CogFeatureSystem.AnchorType);
		}

		public static SoundContext ToSoundContext(this Alignment<ShapeNode> alignment, int seq, int col, Word word, IEnumerable<SoundClass> soundClasses)
		{
			ShapeNode leftNode = GetLeftNode(alignment, word, seq, col);
			SoundClass leftEnv = leftNode == null ? null : leftNode.GetMatchingSoundClass(word.Variety, soundClasses);
			Ngram target = alignment[seq, col].ToNgram(word.Variety);
			ShapeNode rightNode = GetRightNode(alignment, word, seq, col);
			SoundClass rightEnv = rightNode == null ? null : rightNode.GetMatchingSoundClass(word.Variety, soundClasses);
			return new SoundContext(leftEnv, target, rightEnv);
		}

		private static ShapeNode GetLeftNode(Alignment<ShapeNode> alignment, Word word, int seq, int col)
		{
			AlignmentCell<ShapeNode> cell = alignment[seq, col];
			ShapeNode leftNode;
			if (cell.IsNull)
			{
				int index = col - 1;
				while (index >= 0 && alignment[seq, index].Count == 0)
					index--;
				if (index >= 0)
				{
					leftNode = alignment[seq, index].Last;
					if (!NodeFilter(leftNode))
						leftNode = leftNode.GetPrev(NodeFilter);
				}
				else
				{
					leftNode = word.Shape.Begin;
				}
			}
			else
			{
				leftNode = cell.First.GetPrev(NodeFilter);
			}
			return leftNode;
		}

		private static ShapeNode GetRightNode(Alignment<ShapeNode> alignment, Word word, int seq, int col)
		{
			AlignmentCell<ShapeNode> cell = alignment[seq, col];
			ShapeNode rightNode;
			if (cell.IsNull)
			{
				int index = col + 1;
				while (index < alignment.ColumnCount && alignment[seq, index].Count == 0)
					index++;
				if (index < alignment.ColumnCount)
				{
					rightNode = alignment[seq, index].First;
					if (!NodeFilter(rightNode))
						rightNode = rightNode.GetNext(NodeFilter);
				}
				else
				{
					rightNode = word.Shape.End;
				}
			}
			else
			{
				rightNode = cell.Last.GetNext(NodeFilter);
			}
			return rightNode;
		}

		public static Ngram ToNgram(this ShapeNode node, Variety variety)
		{
			return new Ngram(variety.Segments[node]);
		}

		public static Ngram ToNgram(this AlignmentCell<ShapeNode> nodes, Variety variety)
		{
			if (nodes.Count == 0)
				return new Ngram(Segment.Null);
			return new Ngram(nodes.Select(node => variety.Segments[node]));
		}

		public static string ToString(this Alignment<ShapeNode> alignment, IEnumerable<string> notes)
		{
			var sb = new StringBuilder();
			List<string> notesList = notes.ToList();
			bool hasNotes = notesList.Count > 0;
			if (hasNotes)
			{
				while (notesList.Count < alignment.ColumnCount)
					notesList.Add("");
			}

			int maxPrefixLen = alignment.Prefixes.Select(p => p.StrRep()).Concat("").Max(s => s.DisplayLength());
			int[] maxColLens = Enumerable.Range(0, alignment.ColumnCount).Select(c => Enumerable.Range(0, alignment.SequenceCount)
				.Select(s => alignment[s, c].StrRep()).Concat(notesList[c]).Max(s => s.DisplayLength())).ToArray();
			int maxSuffixLen = alignment.Suffixes.Select(s => s.StrRep()).Concat("").Max(s => s.DisplayLength());
			for (int s = 0; s < alignment.SequenceCount; s++)
			{
				AppendSequence(sb, alignment.Prefixes[s].StrRep(), maxPrefixLen, Enumerable.Range(0, alignment.ColumnCount).Select(c => alignment[s, c].StrRep()), maxColLens,
					alignment.Suffixes[s].StrRep(), maxSuffixLen);
			}
			if (hasNotes)
				AppendSequence(sb, "", maxPrefixLen, notesList, maxColLens, "", maxSuffixLen);

			return sb.ToString();
		}

		private static void AppendSequence(StringBuilder sb, string prefix, int maxPrefixLen, IEnumerable<string> columns, int[] maxColLens, string suffix, int maxSuffixLen)
		{
			if (maxPrefixLen > 0)
			{
				sb.Append(prefix.PadRight(maxPrefixLen));
				sb.Append(" ");
			}

			sb.Append("|");
			int index = 0;
			foreach (string col in columns)
			{
				if (index > 0)
					sb.Append(" ");
				sb.Append(col.PadRight(maxColLens[index]));
				index++;
			}
			sb.Append("|");

			if (maxSuffixLen > 0)
			{
				sb.Append(" ");
				sb.Append(suffix.PadRight(maxSuffixLen));
			}
			sb.AppendLine();
		}

		public static int DisplayLength(this string str)
		{
			int len = 0;
			foreach (char c in str)
			{
				switch (CharUnicodeInfo.GetUnicodeCategory(c))
				{
					case UnicodeCategory.NonSpacingMark:
					case UnicodeCategory.SpacingCombiningMark:
					case UnicodeCategory.EnclosingMark:
						break;

					default:
						len++;
						break;
				}
			}
			return len;
		}
	}
}
