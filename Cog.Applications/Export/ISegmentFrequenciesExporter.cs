using SIL.Cog.Applications.ViewModels;
using SIL.Cog.Domain;

namespace SIL.Cog.Applications.Export
{
	public interface ISegmentFrequenciesExporter
	{
		void Export(string path, CogProject project, ViewModelSyllablePosition syllablePosition);
	}
}
