namespace SIL.Cog.ViewModels
{
	public class DataMasterViewModel : MasterViewModelBase
	{
		public DataMasterViewModel(WordListsViewModel wordListsViewModel, VarietiesViewModel varietiesViewModel, SensesViewModel sensesViewModel,
			DataSettingsViewModel dataSettingsViewModel)
			: base("Data", wordListsViewModel, varietiesViewModel, sensesViewModel, dataSettingsViewModel)
		{
		}
	}
}
