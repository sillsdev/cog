using SIL.Cog.Domain;

namespace SIL.Cog.Applications.Import
{
	public interface IWordListsImporter : IImporter
	{
		void Import(object importSettingsViewModel, string path, CogProject project);
	}
}
