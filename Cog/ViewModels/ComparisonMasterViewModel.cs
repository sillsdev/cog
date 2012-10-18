namespace SIL.Cog.ViewModels
{
	public class ComparisonMasterViewModel : MasterViewModelBase
	{
		public ComparisonMasterViewModel(SimilarityMatrixViewModel similarityMatrixViewModel, VarietyPairsViewModel varietyPairsViewModel,
			ComparisonSettingsViewModel comparisonSettingsViewModel)
			: base("Comparison", similarityMatrixViewModel, varietyPairsViewModel, comparisonSettingsViewModel)
		{
		}
	}
}
