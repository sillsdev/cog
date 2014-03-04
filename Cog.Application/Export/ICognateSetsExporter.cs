using System.IO;
using SIL.Cog.Domain;

namespace SIL.Cog.Application.Export
{
	public interface ICognateSetsExporter
	{
		void Export(Stream stream, CogProject project);
	}
}
