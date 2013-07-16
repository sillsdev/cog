using SIL.Cog.Applications.ViewModels;
using SIL.Cog.Domain;

namespace SIL.Cog.Applications.Export
{
	public interface ISimilarityMatrixExporter
	{
		void Export(string path, CogProject project, SimilarityMetric similarityMetric);
	}
}
