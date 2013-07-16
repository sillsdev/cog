namespace SIL.Cog.Applications.ViewModels
{
	public class AnalyzeMasterViewModel : MasterViewModelBase
	{
		public AnalyzeMasterViewModel(HierarchicalGraphViewModel hierarchicalGraphViewModel, NetworkGraphViewModel networkGraphViewModel,
			GeographicalViewModel geographicalViewModel, GlobalCorrespondencesViewModel globalCorrespondencesViewModel)
			: base("Analyze", hierarchicalGraphViewModel, networkGraphViewModel, geographicalViewModel, globalCorrespondencesViewModel)
		{
		}
	}
}
