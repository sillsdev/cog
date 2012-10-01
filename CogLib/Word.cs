using System.Linq;
using System.Text;
using SIL.Collections;
using SIL.Machine;

namespace SIL.Cog
{
	public class Word : IData<ShapeNode>
	{
		private readonly string _strRep;
		private readonly Shape _shape;
		private readonly Sense _sense;

		public Word(string strRep, Shape shape, Sense sense)
		{
			_strRep = strRep;
			_shape = shape;
			_sense = sense;
		}

		public string StrRep
		{
			get { return _strRep; }
		}

		public Shape Shape
		{
			get { return _shape; }
		}

		public Sense Sense
		{
			get { return _sense; }
		}

		public Span<ShapeNode> Span
		{
			get { return Shape.Span; }
		}

		public AnnotationList<ShapeNode> Annotations
		{
			get { return Shape.Annotations; }
		}

		public override string ToString()
		{
			Annotation<ShapeNode> stemAnn = Shape.Annotations.SingleOrDefault(ann => ann.Type() == CogFeatureSystem.StemType);

			var sb = new StringBuilder();
			if (stemAnn != null)
			{
				string prefix = GetString(Shape.First, stemAnn.Span.Start.Prev);
				if (prefix.Length > 0)
				{
					sb.Append(prefix);
					sb.Append(" ");
				}
			}
			sb.Append("|");
			bool first = true;
			foreach (ShapeNode node in stemAnn == null ? Shape : Shape.GetNodes(stemAnn.Span))
			{
				if (!first)
					sb.Append(" ");
				sb.Append((string) node.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep));
				first = false;
			}
			sb.Append("|");

			if (stemAnn != null)
			{
				string suffix = GetString(stemAnn.Span.End.Next, Shape.Last);
				if (suffix.Length > 0)
				{
					sb.Append(" ");
					sb.Append(suffix);
				}
			}

			return sb.ToString();
		}

		private string GetString(ShapeNode startNode, ShapeNode endNode)
		{
			if (startNode == null || endNode == null || startNode == Shape.End || endNode == Shape.Begin)
				return "";

			return string.Concat(startNode.GetNodes(endNode).Select(node => (string)node.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep)));
		}
	}
}
