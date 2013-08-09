namespace SIL.Cog.Applications.ViewModels
{
	public class InputMasterViewModel : MasterViewModelBase
	{
		public InputMasterViewModel(WordListsViewModel wordListsViewModel, VarietiesViewModel varietiesViewModel, SensesViewModel sensesViewModel, SegmentsViewModel segmentsViewModel,
			InputSettingsViewModel inputSettingsViewModel)
			: base("Input", wordListsViewModel, varietiesViewModel, sensesViewModel, segmentsViewModel, inputSettingsViewModel)
		{
		}
	}
}
