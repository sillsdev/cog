using System.Diagnostics;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;

namespace SIL.Cog.Application.ViewModels
{
	public abstract class ComponentSettingsViewModelBase : ChangeTrackingViewModelBase
	{
		private readonly string _displayName;
		private readonly string _wikiTopic;
		private readonly ICommand _helpCommand;

		protected ComponentSettingsViewModelBase(string displayName, string wikiTopic)
		{
			_displayName = displayName;
			_wikiTopic = wikiTopic;
			_helpCommand = new RelayCommand(() => Process.Start(string.Format("https://github.com/sillsdev/cog/wiki/{0}", _wikiTopic)));
		}

		public string DisplayName
		{
			get { return _displayName; }
		}

		public ICommand HelpCommand
		{
			get { return _helpCommand; }
		}

		public abstract void Setup();

		public abstract object UpdateComponent();
	}
}
