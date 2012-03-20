using SIL.Machine;
using System.Linq;
using SIL.Machine.FeatureModel;
using DataStructureExtensions = SIL.Collections.CollectionsExtensions;

namespace SIL.Cog
{
	public static class Extensions
	{
		public static string StrRep(this ShapeNode node)
		{
			return (string) node.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep);
		}

		public static string StrRep(this Annotation<ShapeNode> ann)
		{
			return string.Concat(DataStructureExtensions.GetNodes(ann.Span.Start, ann.Span.End).Select(node => node.StrRep()));
		}

		public static FeatureSymbol Type(this Annotation<ShapeNode> ann)
		{
			return (FeatureSymbol) ann.FeatureStruct.GetValue(CogFeatureSystem.Type);
		}
	}
}
