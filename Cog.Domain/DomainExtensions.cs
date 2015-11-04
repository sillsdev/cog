using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using SIL.Collections;
using SIL.Machine.Annotations;
using SIL.Machine.Clusterers;
using SIL.Machine.FeatureModel;
using SIL.Machine.NgramModeling;
using SIL.Machine.SequenceAlignment;

namespace SIL.Cog.Domain
{
	public static class DomainExtensions
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

		public static bool IsComplex(this ShapeNode node)
		{
			SymbolicFeatureValue sfv;
			if (node.Annotation.FeatureStruct.TryGetValue(CogFeatureSystem.SegmentType, out sfv))
			{
				var symbol = (FeatureSymbol) sfv;
				return symbol == CogFeatureSystem.Complex;
			}
			return false;
		}

		public static bool IsComplex(this Annotation<ShapeNode> ann)
		{
			SymbolicFeatureValue sfv;
			if (ann.FeatureStruct.TryGetValue(CogFeatureSystem.SegmentType, out sfv))
			{
				var symbol = (FeatureSymbol) sfv;
				return symbol == CogFeatureSystem.Complex;
			}
			return false;
		}

		public static SoundContext ToSoundContext(this ShapeNode node, SegmentPool segmentPool, IEnumerable<SoundClass> soundClasses)
		{
			ShapeNode prevNode = node.GetPrev(NodeFilter);
			SoundClass leftEnv;
			if (!soundClasses.TryGetMatchingSoundClass(segmentPool, prevNode, out leftEnv))
				leftEnv = null;
			ShapeNode nextNode = node.GetNext(NodeFilter);
			SoundClass rightEnv;
			if (!soundClasses.TryGetMatchingSoundClass(segmentPool, nextNode, out rightEnv))
				rightEnv = null;
			return new SoundContext(leftEnv, segmentPool.Get(node), rightEnv);
		}

		public static bool TryGetMatchingSoundClass(this IEnumerable<SoundClass> soundClasses, SegmentPool segmentPool, ShapeNode node, out SoundClass soundClass)
		{
			Annotation<ShapeNode> stemAnn = ((Shape) node.List).Annotations.First(ann => ann.Type() == CogFeatureSystem.StemType);
			ShapeNode left = null;
			if (stemAnn.Span.Contains(node) || node.Annotation.CompareTo(stemAnn) > 0)
			{
				ShapeNode leftNode = node.GetPrev(NodeFilter);
				if (leftNode != null)
					left = stemAnn.Span.Contains(leftNode) ? leftNode : node.List.Begin;
			}

			Ngram<Segment> target = stemAnn.Span.Contains(node) ? segmentPool.Get(node) : Segment.Anchor;

			ShapeNode right = null;
			if (stemAnn.Span.Contains(node) || node.Annotation.CompareTo(stemAnn) < 0)
			{
				ShapeNode rightNode = node.GetNext(NodeFilter);
				if (rightNode != null)
					right = stemAnn.Span.Contains(rightNode) ? rightNode : node.List.End;
			}

			soundClass = soundClasses.FirstOrDefault(sc => sc.Matches(left, target, right));
			return soundClass != null;
		}

		public static bool TryGetMatchingSoundClass(this IEnumerable<SoundClass> soundClasses, SegmentPool segmentPool, Alignment<Word, ShapeNode> alignment, int seq, int col, out SoundClass soundClass)
		{
			ShapeNode leftNode = alignment.GetLeftNode(seq, col);
			Ngram<Segment> target = alignment[seq, col].ToNgram(segmentPool);
			ShapeNode rightNode = alignment.GetRightNode(seq, col);
			soundClass = soundClasses.FirstOrDefault(sc => sc.Matches(leftNode, target, rightNode));
			return soundClass != null;
		}

		private static bool NodeFilter(ShapeNode node)
		{
			return node.Type().IsOneOf(CogFeatureSystem.VowelType, CogFeatureSystem.ConsonantType, CogFeatureSystem.AnchorType);
		}

		public static SoundContext ToSoundContext(this Alignment<Word, ShapeNode> alignment, SegmentPool segmentPool, int seq, int col, IEnumerable<SoundClass> soundClasses)
		{
			ShapeNode leftNode = alignment.GetLeftNode(seq, col);
			SoundClass leftEnv;
			if (leftNode == null || !soundClasses.TryGetMatchingSoundClass(segmentPool, leftNode, out leftEnv))
				leftEnv = null;
			Ngram<Segment> target = alignment[seq, col].ToNgram(segmentPool);
			ShapeNode rightNode = alignment.GetRightNode(seq, col);
			SoundClass rightEnv;
			if (rightNode == null || !soundClasses.TryGetMatchingSoundClass(segmentPool, rightNode, out rightEnv))
				rightEnv = null;
			return new SoundContext(leftEnv, target, rightEnv);
		}

		public static ShapeNode GetLeftNode(this Alignment<Word, ShapeNode> alignment, int seq, int col)
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
					leftNode = alignment.Sequences[seq].Shape.Begin;
				}
			}
			else
			{
				leftNode = cell.First.GetPrev(NodeFilter);
			}
			return leftNode;
		}

		public static ShapeNode GetRightNode(this Alignment<Word, ShapeNode> alignment, int seq, int col)
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
					rightNode = alignment.Sequences[seq].Shape.End;
				}
			}
			else
			{
				rightNode = cell.Last.GetNext(NodeFilter);
			}
			return rightNode;
		}

		public static Ngram<Segment> ToNgram(this IEnumerable<ShapeNode> nodes, SegmentPool segmentPool)
		{
			return new Ngram<Segment>(nodes.Select(segmentPool.Get));
		}

		public static string ToString(this Alignment<Word, ShapeNode> alignment, IEnumerable<string> notes)
		{
			var sb = new StringBuilder();
			List<string> notesList = notes.ToList();
			bool hasNotes = notesList.Count > 0;
			while (notesList.Count < alignment.ColumnCount)
				notesList.Add("");

			int maxPrefixLen = alignment.Prefixes.Select(p => p.StrRep()).Concat("").Max(s => s.DisplayLength());
			int[] maxColLens = Enumerable.Range(0, alignment.ColumnCount).Select(c => Enumerable.Range(0, alignment.SequenceCount)
				.Select(s => alignment[s, c].StrRep()).Concat(notesList[c]).Max(s => s.DisplayLength())).ToArray();
			int maxSuffixLen = alignment.Suffixes.Select(s => s.StrRep()).Concat("").Max(s => s.DisplayLength());
			for (int s = 0; s < alignment.SequenceCount; s++)
			{
				AppendSequence(sb, alignment.Prefixes[s].StrRep(), maxPrefixLen, Enumerable.Range(0, alignment.ColumnCount).Select(c => alignment[s, c].IsNull ? "-" : alignment[s, c].StrRep()), maxColLens,
					alignment.Suffixes[s].StrRep(), maxSuffixLen, "|");
			}
			if (hasNotes)
				AppendSequence(sb, "", maxPrefixLen, notesList, maxColLens, "", maxSuffixLen, " ");

			return sb.ToString();
		}

		private static void AppendSequence(StringBuilder sb, string prefix, int maxPrefixLen, IEnumerable<string> columns, int[] maxColLens, string suffix, int maxSuffixLen, string separator)
		{
			if (maxPrefixLen > 0)
			{
				sb.Append(prefix.PadRight(maxPrefixLen));
				sb.Append(" ");
			}

			sb.Append(separator);
			int index = 0;
			foreach (string col in columns)
			{
				if (index > 0)
					sb.Append(" ");
				sb.Append(col.PadRight(maxColLens[index]));
				index++;
			}
			sb.Append(separator);

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

		public static string GetString(this FeatureStruct fs)
		{
			var sb = new StringBuilder();
			sb.Append("[");
			bool firstFeature = true;
			foreach (SymbolicFeature feature in fs.Features.Where(f => !CogFeatureSystem.Instance.ContainsFeature(f)).OfType<SymbolicFeature>())
			{
				if (!firstFeature)
					sb.Append(",");
				sb.Append(feature.Description);
				sb.Append(":");
				SymbolicFeatureValue fv = fs.GetValue(feature);
				FeatureSymbol[] symbols = fv.Values.ToArray();
				if (symbols.Length > 1)
					sb.Append("{");
				bool firstSymbol = true;
				foreach (FeatureSymbol symbol in symbols)
				{
					if (!firstSymbol)
						sb.Append(",");
					sb.Append(symbol.Description);
					firstSymbol = false;
				}
				if (symbols.Length > 1)
					sb.Append("}");
				firstFeature = false;
			}
			sb.Append("]");
			return sb.ToString();
		}

		public static IEnumerable<Cluster<Word>> GenerateCognateSets(this CogProject project, Meaning meaning)
		{
			var words = new HashSet<Word>();
			var noise = new HashSet<Word>();
			foreach (VarietyPair vp in project.VarietyPairs)
			{
				WordPair wp;
				if (vp.WordPairs.TryGetValue(meaning, out wp))
				{
					if (wp.AreCognatePredicted)
					{
						words.Add(wp.Word1);
						words.Add(wp.Word2);
						noise.Remove(wp.Word1);
						noise.Remove(wp.Word2);
					}
					else
					{
						if (!words.Contains(wp.Word1))
							noise.Add(wp.Word1);
						if (!words.Contains(wp.Word2))
							noise.Add(wp.Word2);
					}
				}
			}

			double min = double.MaxValue;
			var distanceMatrix = new Dictionary<UnorderedTuple<Word, Word>, double>();
			Word[] wordArray = words.ToArray();
			for (int i = 0; i < wordArray.Length; i++)
			{
				for (int j = i + 1; j < wordArray.Length; j++)
				{
					Word w1 = wordArray[i];
					Word w2 = wordArray[j];
					double score = 0;
					WordPair wp;
					if (w1.Variety != w2.Variety && w1.Variety.VarietyPairs[w2.Variety].WordPairs.TryGetValue(meaning, out wp) && wp.AreCognatePredicted
					    && wp.GetWord(w1.Variety) == w1 && wp.GetWord(w2.Variety) == w2)
					{
						score = wp.CognacyScore;
					}
					double distance = 1.0 - score;
					min = Math.Min(min, distance);
					distanceMatrix[UnorderedTuple.Create(w1, w2)] = distance;
				}
			}

			var clusterer = new FlatUpgmaClusterer<Word>((w1, w2) => distanceMatrix[UnorderedTuple.Create(w1, w2)], (1.0 + min) / 2);
			return clusterer.GenerateClusters(words).Concat(new Cluster<Word>(noise, true));
		}
	}
}
