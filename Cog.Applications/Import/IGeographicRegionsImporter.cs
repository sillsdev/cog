using SIL.Cog.Domain;

namespace SIL.Cog.Applications.Import
{
	public interface IGeographicRegionsImporter : IImporter
	{
		void Import(object importSettingsViewModel, string path, CogProject project);
	}
}
