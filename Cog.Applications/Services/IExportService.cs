using SIL.Cog.Applications.ViewModels;
using SIL.Cog.Domain;

namespace SIL.Cog.Applications.Services
{
	public interface IExportService
	{
		bool ExportSimilarityMatrix(object ownerViewModel, SimilarityMetric similarityMetric);
		bool ExportWordLists(object ownerViewModel);
		bool ExportCognateSets(object ownerViewModel);
		bool ExportVarietyPair(object ownerViewModel, VarietyPair varietyPair);
		bool ExportSegmentFrequencies(object ownerViewModel, ViewModelSyllablePosition syllablePosition);
	}
}
