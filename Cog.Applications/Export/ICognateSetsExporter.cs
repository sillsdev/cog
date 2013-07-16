using SIL.Cog.Domain;

namespace SIL.Cog.Applications.Export
{
	public interface ICognateSetsExporter
	{
		void Export(string path, CogProject project);
	}
}
