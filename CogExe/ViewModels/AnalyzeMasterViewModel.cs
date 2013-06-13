namespace SIL.Cog.ViewModels
{
	public class AnalyzeMasterViewModel : MasterViewModelBase
	{
		public AnalyzeMasterViewModel(HierarchicalGraphViewModel hierarchicalGraphViewModel, NetworkGraphViewModel networkGraphViewModel,
			GeographicalViewModel geographicalViewModel, SimilarSegmentsViewModel similarSegmentsViewModel)
			: base("Analyze", hierarchicalGraphViewModel, networkGraphViewModel, geographicalViewModel, similarSegmentsViewModel)
		{
		}
	}
}
