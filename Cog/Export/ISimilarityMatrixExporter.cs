using SIL.Cog.ViewModels;

namespace SIL.Cog.Export
{
	public interface ISimilarityMatrixExporter
	{
		void Export(string path, CogProject project, SimilarityMetric similarityMetric);
	}
}
