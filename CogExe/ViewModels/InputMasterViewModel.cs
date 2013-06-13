namespace SIL.Cog.ViewModels
{
	public class InputMasterViewModel : MasterViewModelBase
	{
		public InputMasterViewModel(WordListsViewModel wordListsViewModel, VarietiesViewModel varietiesViewModel, SensesViewModel sensesViewModel,
			InputSettingsViewModel inputSettingsViewModel)
			: base("Input", wordListsViewModel, varietiesViewModel, sensesViewModel, inputSettingsViewModel)
		{
		}
	}
}
