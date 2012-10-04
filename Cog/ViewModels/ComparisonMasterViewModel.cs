namespace SIL.Cog.ViewModels
{
	public class ComparisonMasterViewModel : MasterViewModelBase
	{
		public ComparisonMasterViewModel(SimilarityMatrixViewModel similarityMatrixViewModel, VarietyPairsViewModel varietyPairsViewModel)
			: base("Comparison", similarityMatrixViewModel, varietyPairsViewModel)
		{
		}
	}
}
