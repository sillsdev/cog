using SIL.Cog.Application.ViewModels;

namespace SIL.Cog.Application.Services
{
	public interface IImageExportService
	{
		bool ExportCurrentHierarchicalGraph(object ownerViewModel, HierarchicalGraphType type);
		bool ExportHierarchicalGraph(object ownerViewModel, HierarchicalGraphType graphType, ClusteringMethod clusteringMethod, SimilarityMetric similarityMetric);
		bool ExportCurrentNetworkGraph(object ownerViewModel);
		bool ExportNetworkGraph(object ownerViewModel, SimilarityMetric similarityMetric, double scoreFilter);
		bool ExportCurrentMap(object ownerViewModel);
		bool ExportGlobalCorrespondencesChart(object ownerViewModel, SyllablePosition syllablePosition, int frequencyFilter);
		bool ExportCurrentGlobalCorrespondencesChart(object ownerViewModel);
	}
}
