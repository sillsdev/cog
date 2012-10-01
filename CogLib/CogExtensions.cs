using System.Linq;
using SIL.Collections;
using SIL.Machine;
using SIL.Machine.FeatureModel;

namespace SIL.Cog
{
	public static class CogExtensions
	{
		public static string StrRep(this ShapeNode node)
		{
			return (string) node.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep);
		}

		public static string StrRep(this Annotation<ShapeNode> ann)
		{
			return string.Concat(ann.Span.Start.GetNodes(ann.Span.End).Select(node => node.StrRep()));
		}

		public static FeatureSymbol Type(this Annotation<ShapeNode> ann)
		{
			return (FeatureSymbol) ann.FeatureStruct.GetValue(CogFeatureSystem.Type);
		}
	}
}
