namespace SIL.Cog.Import
{
	public interface IWordListsImporter : IImporter
	{
		void Import(object importSettingsViewModel, string path, CogProject project);
	}
}
