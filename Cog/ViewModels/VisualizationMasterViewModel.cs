namespace SIL.Cog.ViewModels
{
	public class VisualizationMasterViewModel : MasterViewModelBase
	{
		public VisualizationMasterViewModel(HierarchicalGraphViewModel hierarchicalGraphViewModel, NetworkGraphViewModel networkGraphViewModel)
			: base("Visualization", hierarchicalGraphViewModel, networkGraphViewModel)
		{
		}
	}
}
