using System.Linq;
using System.Text;
using SIL.Machine;

namespace SIL.Cog
{
	public class Word
	{
		private readonly Shape _shape;
		private readonly string _language;
		private readonly string _gloss;

		public Word(Shape shape, string language, string gloss)
		{
			_shape = shape;
			_language = language;
			_gloss = gloss;
		}

		public Shape Shape
		{
			get { return _shape; }
		}

		public string Language
		{
			get { return _language; }
		}

		public string Gloss
		{
			get { return _gloss; }
		}

		public override string ToString()
		{
			return _shape.Aggregate(new StringBuilder(),
				(sb, node) => sb.Append((string) node.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep))).ToString();
		}
	}
}
