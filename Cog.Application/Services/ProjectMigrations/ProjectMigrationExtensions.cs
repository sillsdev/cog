using SIL.Cog.Domain;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Application.Services.ProjectMigrations
{
	internal static class ProjectMigrationExtensions
	{
		public static void AddSymbolBasedOn(this SymbolCollection symbols, string strRep, string basedOnStrRep, params FeatureSymbol[] values)
		{
			Symbol basedOnSymbol;
			if (!symbols.Contains(strRep)
				&& symbols.TryGet(basedOnStrRep, out basedOnSymbol))
			{
				FeatureStruct fs = basedOnSymbol.FeatureStruct.Clone();
				foreach (FeatureSymbol value in values)
					fs.AddValue(value.Feature, value);
				fs.Freeze();
				symbols.Add(strRep, fs);
			}
		}
	}
}
