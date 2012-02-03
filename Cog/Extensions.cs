using SIL.Machine;
using System.Linq;

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
			return string.Concat(ann.Span.Start.GetNodes(ann.Span.End).Select(node => node.StrRep()));
		}
	}
}
