namespace SIL.Cog.Application.ViewModels
{
	public class InputViewModel : ContainerViewModelBase
	{
		public InputViewModel(WordListsViewModel wordLists, VarietiesViewModel varieties, MeaningsViewModel meanings, SegmentsViewModel segments,
			InputSettingsViewModel inputSettings)
			: base("Input", wordLists, varieties, meanings, segments, inputSettings)
		{
		}
	}
}
