using SIL.Collections;

namespace SIL.Cog.Applications.ViewModels
{
	public class TaskAreaCommandGroupViewModel : TaskAreaViewModelBase
	{
		private readonly ReadOnlyList<TaskAreaCommandViewModel> _commands;
		private TaskAreaCommandViewModel _currentCommand;

		public TaskAreaCommandGroupViewModel(params TaskAreaCommandViewModel[] commands)
			: this(null, commands)
		{
			
		} 

		public TaskAreaCommandGroupViewModel(string displayName, params TaskAreaCommandViewModel[] commands)
			: base(displayName)
		{
			_commands = new ReadOnlyList<TaskAreaCommandViewModel>(commands);
			if (_commands.Count > 0)
				_currentCommand = _commands[0];
		}

		public ReadOnlyList<TaskAreaCommandViewModel> Commands
		{
			get { return _commands; }
		}

		public TaskAreaCommandViewModel CurrentCommand
		{
			get { return _currentCommand; }
			set { Set(() => CurrentCommand, ref _currentCommand, value); }
		}
	}
}
