namespace SIL.Cog.ViewModels
{
	public class CompareMasterViewModel : MasterViewModelBase
	{
		public CompareMasterViewModel(SimilarityMatrixViewModel similarityMatrixViewModel, VarietyPairsViewModel varietyPairsViewModel, SenseAlignmentViewModel senseAlignmentViewModel,
			CompareSettingsViewModel compareSettingsViewModel)
			: base("Compare", similarityMatrixViewModel, varietyPairsViewModel, senseAlignmentViewModel, compareSettingsViewModel)
		{
		}
	}
}
