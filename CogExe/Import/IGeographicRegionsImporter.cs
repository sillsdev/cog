namespace SIL.Cog.Import
{
	public interface IGeographicRegionsImporter : IImporter
	{
		void Import(object importSettingsViewModel, string path, CogProject project);
	}
}
