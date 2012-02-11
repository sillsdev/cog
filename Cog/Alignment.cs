using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
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
			Annotation<ShapeNode> ann1 = _shape1.Annotations.SingleOrDefault(ann => ann.Type() == CogFeatureSystem.StemType);
			Annotation<ShapeNode> ann2 = _shape2.Annotations.SingleOrDefault(ann => ann.Type() == CogFeatureSystem.StemType);

			var sb = new StringBuilder();
			if (ann1 == null || ann2 == null)
			{
				sb.AppendLine(GetString(_shape1, _shape1.First, _shape1.Last));
				sb.AppendLine(GetString(_shape2, _shape2.First, _shape2.Last));
			}
			else
			{
				string prefix1 = GetString(_shape1, _shape1.First, ann1.Span.Start.Prev);
				string prefix2 = GetString(_shape2, _shape2.First, ann2.Span.Start.Prev);

				string suffix1 = GetString(_shape1, ann1.Span.End.Next, _shape1.Last);
				string suffix2 = GetString(_shape2, ann2.Span.End.Next, _shape2.Last);

				if (prefix1.Length > 0 || prefix2.Length > 0)
				{
					sb.Append(prefix1.PadRight(Math.Max(GetLength(prefix1), GetLength(prefix2))));
					sb.Append(" ");
				}
				sb.Append("|");
				bool first = true;
				foreach (Tuple<Annotation<ShapeNode>, Annotation<ShapeNode>> tuple in ann1.Children.Zip(ann2.Children))
				{
					if (!first)
						sb.Append(" ");
					string strRep1 = tuple.Item1.StrRep();
					string strRep2 = tuple.Item2.StrRep();
					sb.Append(strRep1.PadRight(Math.Max(GetLength(strRep1), GetLength(strRep2))));
					first = false;
				}
				sb.Append("|");
				if (suffix1.Length > 0)
				{
					sb.Append(" ");
					sb.Append(suffix1.PadRight(Math.Max(GetLength(suffix1), GetLength(suffix2))));
				}
				sb.AppendLine();
				if (prefix1.Length > 0 || prefix2.Length > 0)
				{
					sb.Append(prefix2.PadRight(Math.Max(GetLength(prefix1), GetLength(prefix2))));
					sb.Append(" ");
				}
				sb.Append("|");
				first = true;
				foreach (Tuple<Annotation<ShapeNode>, Annotation<ShapeNode>> tuple in ann1.Children.Zip(ann2.Children))
				{
					if (!first)
						sb.Append(" ");
					string strRep1 = tuple.Item1.StrRep();
					string strRep2 = tuple.Item2.StrRep();
					sb.Append(strRep2.PadRight(Math.Max(GetLength(strRep1), GetLength(strRep2))));
					first = false;
				}
				sb.Append("|");
				if (suffix2.Length > 0)
				{
					sb.Append(" ");
					sb.Append(suffix2.PadRight(Math.Max(GetLength(suffix1), GetLength(suffix2))));
				}
				sb.AppendLine();
			}
			return sb.ToString();
		}

		private string GetString(Shape shape, ShapeNode startNode, ShapeNode endNode)
		{
			if (startNode == null || endNode == null || startNode == shape.End || endNode == shape.Begin)
				return "";

			return string.Concat(startNode.GetNodes(endNode).Select(node => (string) node.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep)));
		}

		private int GetLength(string str)
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
