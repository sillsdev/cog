namespace SIL.Cog.ViewModels
{
	public class VisualizationMasterViewModel : MasterViewModelBase
	{
		public VisualizationMasterViewModel(HierarchicalGraphViewModel hierarchicalGraphViewModel, NetworkGraphViewModel networkGraphViewModel, GeographicalViewModel geographicalViewModel)
			: base("Visualization", hierarchicalGraphViewModel, networkGraphViewModel, geographicalViewModel)
		{
		}
	}
}
