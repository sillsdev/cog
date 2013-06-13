namespace SIL.Cog.ViewModels
{
	public class CompareMasterViewModel : MasterViewModelBase
	{
		public CompareMasterViewModel(SimilarityMatrixViewModel similarityMatrixViewModel, VarietyPairsViewModel varietyPairsViewModel,
			CompareSettingsViewModel compareSettingsViewModel)
			: base("Compare", similarityMatrixViewModel, varietyPairsViewModel, compareSettingsViewModel)
		{
		}
	}
}
