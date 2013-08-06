using SIL.Cog.Applications.ViewModels;

namespace SIL.Cog.Applications.Services
{
	public interface IImageExportService
	{
		bool ExportCurrentHierarchicalGraph(object ownerViewModel, HierarchicalGraphType type);
		bool ExportHierarchicalGraph(object ownerViewModel, HierarchicalGraphType graphType, ClusteringMethod clusteringMethod, SimilarityMetric similarityMetric);
		bool ExportCurrentNetworkGraph(object ownerViewModel);
		bool ExportNetworkGraph(object ownerViewModel, SimilarityMetric similarityMetric, double scoreFilter);
		bool ExportCurrentMap(object ownerViewModel);
		bool ExportGlobalCorrespondencesChart(object ownerViewModel, SoundCorrespondenceType correspondenceType, int frequencyFilter);
		bool ExportCurrentGlobalCorrespondencesChart(object ownerViewModel);
	}
}
