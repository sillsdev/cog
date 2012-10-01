namespace SIL.Cog.ViewModels
{
	public class DataSettingsViewModel : MasterViewModel
	{
		public DataSettingsViewModel(StemmerSettingsViewModel stemmerSettingsViewModel)
			: base("Settings", stemmerSettingsViewModel)
		{
		}

		public override void Initialize(CogProject project)
		{
		}
	}
}
