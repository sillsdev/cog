using System.IO;
using SIL.Cog.Domain;

namespace SIL.Cog.Application.Import
{
	public interface IWordListsImporter : IImporter
	{
		void Import(object importSettingsViewModel, Stream stream, CogProject project);
	}
}
