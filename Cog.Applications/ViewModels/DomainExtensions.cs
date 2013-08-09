using System.Linq;
using SIL.Cog.Domain;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Applications.ViewModels
{
	internal static class DomainExtensions
	{
		public static bool IsComplex(this Segment segment)
		{
			if (segment.Type == CogFeatureSystem.ConsonantType)
			{
				var placeValue = segment.FeatureStruct.GetValue<SymbolicFeatureValue>("place");
				return placeValue.Values.Count() > 1;
			}
			else
			{
				var heightValue = segment.FeatureStruct.GetValue<SymbolicFeatureValue>("height");
				var backnessValue = segment.FeatureStruct.GetValue<SymbolicFeatureValue>("backness");
				return heightValue.Values.Count() > 1 || backnessValue.Values.Count() > 1;
			}
		}
	}
}
