namespace SIL.Cog.Applications.ViewModels
{
	public class AnalyzeViewModel : ContainerViewModelBase
	{
		public AnalyzeViewModel(HierarchicalGraphViewModel hierarchicalGraph, NetworkGraphViewModel networkGraph, GeographicalViewModel geographical,
			GlobalCorrespondencesViewModel globalCorrespondences)
			: base("Analyze", hierarchicalGraph, networkGraph, geographical, globalCorrespondences)
		{
		}
	}
}
