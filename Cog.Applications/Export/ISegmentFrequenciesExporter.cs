using System.IO;
using SIL.Cog.Applications.ViewModels;
using SIL.Cog.Domain;

namespace SIL.Cog.Applications.Export
{
	public interface ISegmentFrequenciesExporter
	{
		void Export(Stream stream, CogProject project, ViewModelSyllablePosition syllablePosition);
	}
}
