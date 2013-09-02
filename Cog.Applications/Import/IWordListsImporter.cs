using System.IO;
using SIL.Cog.Domain;

namespace SIL.Cog.Applications.Import
{
	public interface IWordListsImporter : IImporter
	{
		void Import(object importSettingsViewModel, Stream stream, CogProject project);
	}
}
