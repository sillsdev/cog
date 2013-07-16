namespace SIL.Cog.Applications.ViewModels
{
	public class CompareMasterViewModel : MasterViewModelBase
	{
		public CompareMasterViewModel(SimilarityMatrixViewModel similarityMatrixViewModel, VarietyPairsViewModel varietyPairsViewModel, MultipleWordAlignmentViewModel multipleWordAlignmentViewModel,
			CompareSettingsViewModel compareSettingsViewModel)
			: base("Compare", similarityMatrixViewModel, varietyPairsViewModel, multipleWordAlignmentViewModel, compareSettingsViewModel)
		{
		}
	}
}
