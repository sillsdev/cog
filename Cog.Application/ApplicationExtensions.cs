using System.Collections.Generic;
using SIL.Cog.Domain;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Application
{
	public static class ApplicationExtensions
	{
		private readonly static Dictionary<string, string> PlaceCategoryLookup = new Dictionary<string, string>
			{
				{"bilabial", "Labial"},
				{"labiodental", "Labial"},
				{"dental", "Coronal"},
				{"alveolar", "Coronal"},
				{"retroflex", "Coronal"},
				{"palato-alveolar", "Coronal"},
				{"alveolo-palatal", "Coronal"},
				{"palatal", "Dorsal"},
				{"velar", "Dorsal"},
				{"uvular", "Dorsal"},
				{"pharyngeal", "Guttural"},
				{"epiglottal", "Guttural"},
				{"glottal", "Guttural"}
			};

		private readonly static Dictionary<string, string> HeightCategoryLookup = new Dictionary<string, string>
			{
				{"close-vowel", "Close"},
				{"mid-vowel", "Mid"},
				{"open-vowel", "Open"}
			};

		public static string Category(this Segment segment)
		{
			FeatureStruct fs = segment.FeatureStruct;
			if (segment.IsComplex)
				fs = segment.FeatureStruct.GetValue(CogFeatureSystem.First);

			return segment.Type == CogFeatureSystem.VowelType ? HeightCategoryLookup[((FeatureSymbol) fs.GetValue<SymbolicFeatureValue>("manner")).ID]
				: PlaceCategoryLookup[((FeatureSymbol) fs.GetValue<SymbolicFeatureValue>("place")).ID];
		}
	}
}
