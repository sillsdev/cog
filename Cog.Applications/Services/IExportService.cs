using SIL.Cog.Applications.ViewModels;
using SIL.Cog.Domain;

namespace SIL.Cog.Applications.Services
{
	public interface IExportService
	{
		bool ExportSimilarityMatrix(object ownerViewModel, CogProject project, SimilarityMetric similarityMetric);
		bool ExportWordLists(object ownerViewModel, CogProject project);
		bool ExportCognateSets(object ownerViewModel, CogProject project);
		bool ExportVarietyPair(object ownerViewModel, CogProject project, VarietyPair varietyPair);
	}
}
