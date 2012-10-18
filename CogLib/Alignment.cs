using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using SIL.Collections;
using SIL.Machine;

namespace SIL.Cog
{
	public class Alignment
	{
		private readonly Shape _shape1;
		private readonly Shape _shape2;
		private readonly double _score;

		public Alignment(Shape shape1, Shape shape2, double score)
		{
			_shape1 = shape1;
			_shape2 = shape2;
			_score = score;
		}

		public Shape Shape1
		{
			get { return _shape1; }
		}

		public Shape Shape2
		{
			get { return _shape2; }
		}

		public double Score
		{
			get { return _score; }
		}

		public Annotation<ShapeNode> Prefix1
		{
			get
			{
				return _shape1.Annotations.SingleOrDefault(ann => ann.Type() == CogFeatureSystem.PrefixType);
			}
		}

		public Annotation<ShapeNode> Prefix2
		{
			get
			{
				return _shape2.Annotations.SingleOrDefault(ann => ann.Type() == CogFeatureSystem.PrefixType);
			}
		}

		public Annotation<ShapeNode> Suffix1
		{
			get
			{
				return _shape1.Annotations.SingleOrDefault(ann => ann.Type() == CogFeatureSystem.SuffixType);
			}
		}

		public Annotation<ShapeNode> Suffix2
		{
			get
			{
				return _shape2.Annotations.SingleOrDefault(ann => ann.Type() == CogFeatureSystem.SuffixType);
			}
		}

		public IEnumerable<Tuple<Annotation<ShapeNode>, Annotation<ShapeNode>>> AlignedAnnotations
		{
			get
			{
				Annotation<ShapeNode> ann1 = _shape1.Annotations.SingleOrDefault(ann => ann.Type() == CogFeatureSystem.StemType);
				Annotation<ShapeNode> ann2 = _shape2.Annotations.SingleOrDefault(ann => ann.Type() == CogFeatureSystem.StemType);
				if (ann1 != null && ann2 != null)
					return ann1.Children.Zip(ann2.Children);
				return Enumerable.Empty<Tuple<Annotation<ShapeNode>, Annotation<ShapeNode>>>();
			}
		}

		public override string ToString()
		{
			return ToString(Enumerable.Empty<string>());
		}

		public string ToString(IEnumerable<string> notes)
		{
			Annotation<ShapeNode> stemAnn1 = _shape1.Annotations.SingleOrDefault(ann => ann.Type() == CogFeatureSystem.StemType);
			Annotation<ShapeNode> stemAnn2 = _shape2.Annotations.SingleOrDefault(ann => ann.Type() == CogFeatureSystem.StemType);

			var sb = new StringBuilder();
			if (stemAnn1 == null || stemAnn2 == null)
			{
				sb.AppendLine(GetString(_shape1, _shape1.First, _shape1.Last));
				sb.AppendLine(GetString(_shape2, _shape2.First, _shape2.Last));
			}
			else
			{
				List<string> notesList = notes.ToList();
				bool noNotes = notesList.Count == 0;
				while (notesList.Count < stemAnn1.Children.Count)
					notesList.Add("");

				string prefix1 = GetString(Prefix1);
				string prefix2 = GetString(Prefix2);

				string suffix1 = GetString(Suffix1);
				string suffix2 = GetString(Suffix2);

				if (prefix1.Length > 0 || prefix2.Length > 0)
				{
					sb.Append(PadString(prefix1, prefix2, ""));
					sb.Append(" ");
				}
				sb.Append("|");
				bool first = true;
				foreach (Tuple<Annotation<ShapeNode>, Annotation<ShapeNode>, string> tuple in stemAnn1.Children.Zip(stemAnn2.Children, notesList))
				{
					if (!first)
						sb.Append(" ");
					sb.Append(PadString(tuple.Item1.StrRep(), tuple.Item2.StrRep(), tuple.Item3));
					first = false;
				}
				sb.Append("|");
				if (suffix1.Length > 0)
				{
					sb.Append(" ");
					sb.Append(PadString(suffix1, suffix2, ""));
				}
				sb.AppendLine();
				if (prefix1.Length > 0 || prefix2.Length > 0)
				{
					sb.Append(PadString(prefix2, prefix1, ""));
					sb.Append(" ");
				}
				sb.Append("|");
				first = true;
				foreach (Tuple<Annotation<ShapeNode>, Annotation<ShapeNode>, string> tuple in stemAnn1.Children.Zip(stemAnn2.Children, notesList))
				{
					if (!first)
						sb.Append(" ");
					sb.Append(PadString(tuple.Item2.StrRep(), tuple.Item1.StrRep(), tuple.Item3));
					first = false;
				}
				sb.Append("|");
				if (suffix2.Length > 0)
				{
					sb.Append(" ");
					sb.Append(PadString(suffix2, suffix1, ""));
				}
				sb.AppendLine();

				if (!noNotes)
				{
					if (prefix1.Length > 0 || prefix2.Length > 0)
					{
						sb.Append(PadString("", prefix1, prefix2));
						sb.Append(" ");
					}

					sb.Append("|");
					first = true;
					foreach (Tuple<Annotation<ShapeNode>, Annotation<ShapeNode>, string> tuple in stemAnn1.Children.Zip(stemAnn2.Children, notesList))
					{
						if (!first)
							sb.Append(" ");
						sb.Append(PadString(tuple.Item3, tuple.Item1.StrRep(), tuple.Item2.StrRep()));
						first = false;
					}
					sb.Append("|");
					if (suffix2.Length > 0)
					{
						sb.Append(" ");
						sb.Append(PadString("", suffix1, suffix2));
					}
					sb.AppendLine();
				}
			}
			return sb.ToString();
		}

		private static string GetString(Shape shape, ShapeNode startNode, ShapeNode endNode)
		{
			if (startNode == null || endNode == null || startNode == shape.End || endNode == shape.Begin)
				return "";

			return string.Concat(startNode.GetNodes(endNode).Select(node => (string) node.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep)));
		}

		private static string GetString(Annotation<ShapeNode> ann)
		{
			if (ann == null)
				return "";
			return ann.StrRep();
		}

		private static string PadString(string str, params string[] strs)
		{
			int len = GetLength(str);
			int maxLen = strs.Select(GetLength).Concat(len).Max();
			var sb = new StringBuilder();
			sb.Append(str);
			for (int i = 0; i < maxLen - len; i++)
				sb.Append(" ");

			return sb.ToString();
		}

		private static int GetLength(string str)
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
