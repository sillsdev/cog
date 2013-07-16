using SIL.Cog.Applications.Services;
using SIL.Cog.Presentation.Properties;

namespace SIL.Cog.Presentation.Services
{
	public class SettingsService : ISettingsService
	{
		public string LastProject
		{
			get { return Settings.Default.LastProject; }
			set { Settings.Default.LastProject = value; }
		}
	}
}
