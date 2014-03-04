using System.IO;
using SIL.Cog.Domain;

namespace SIL.Cog.Application.Export
{
	public interface IWordListsExporter
	{
		void Export(Stream stream, CogProject project);
	}
}
