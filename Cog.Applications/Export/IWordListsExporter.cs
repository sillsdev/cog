using SIL.Cog.Domain;

namespace SIL.Cog.Applications.Export
{
	public interface IWordListsExporter
	{
		void Export(string path, CogProject project);
	}
}
