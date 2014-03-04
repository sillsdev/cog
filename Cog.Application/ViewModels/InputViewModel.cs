namespace SIL.Cog.Application.ViewModels
{
	public class InputViewModel : ContainerViewModelBase
	{
		public InputViewModel(WordListsViewModel wordLists, VarietiesViewModel varieties, SensesViewModel senses, SegmentsViewModel segments,
			InputSettingsViewModel inputSettings)
			: base("Input", wordLists, varieties, senses, segments, inputSettings)
		{
		}
	}
}
