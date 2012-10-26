using System.Collections.ObjectModel;

namespace SIL.Cog.ViewModels
{
	public class TaskAreaViewModel : CogViewModelBase
	{
		private readonly ReadOnlyCollection<CommandViewModel> _commands;

		public TaskAreaViewModel(string displayName, params CommandViewModel[] commands)
			: base(displayName)
		{
			_commands = new ReadOnlyCollection<CommandViewModel>(commands);
		}

		public ReadOnlyCollection<CommandViewModel> Commands
		{
			get { return _commands; }
		}
	}
}
