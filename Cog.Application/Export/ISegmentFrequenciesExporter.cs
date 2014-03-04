using System.IO;
using SIL.Cog.Application.ViewModels;
using SIL.Cog.Domain;

namespace SIL.Cog.Application.Export
{
	public interface ISegmentFrequenciesExporter
	{
		void Export(Stream stream, CogProject project, SyllablePosition syllablePosition);
	}
}
