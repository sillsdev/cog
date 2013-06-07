namespace SIL.Cog.ViewModels
{
	public class ComparisonMasterViewModel : MasterViewModelBase
	{
		public ComparisonMasterViewModel(SimilarityMatrixViewModel similarityMatrixViewModel, VarietyPairsViewModel varietyPairsViewModel,
			SimilarSegmentsViewModel similarSegmentsViewModel, ComparisonSettingsViewModel comparisonSettingsViewModel)
			: base("Comparison", similarityMatrixViewModel, varietyPairsViewModel, similarSegmentsViewModel, comparisonSettingsViewModel)
		{
		}
	}
}
