namespace SIL.Cog.Applications.ViewModels
{
	public class CompareViewModel : ContainerViewModelBase
	{
		public CompareViewModel(SimilarityMatrixViewModel similarityMatrix, VarietyPairsViewModel varietyPairs, MultipleWordAlignmentViewModel multipleWordAlignment,
			CompareSettingsViewModel compareSettings)
			: base("Compare", similarityMatrix, varietyPairs, multipleWordAlignment, compareSettings)
		{
		}
	}
}
