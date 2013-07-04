using SIL.Collections;

namespace SIL.Cog.ViewModels
{
	public class TaskAreaCommandsViewModel : CogViewModelBase
	{
		private readonly ReadOnlyList<CommandViewModel> _commands;

		public TaskAreaCommandsViewModel(string displayName, params CommandViewModel[] commands)
			: base(displayName)
		{
			_commands = new ReadOnlyList<CommandViewModel>(commands);
		}

		public ReadOnlyList<CommandViewModel> Commands
		{
			get { return _commands; }
		}
	}
}
