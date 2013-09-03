using SIL.Collections;

namespace SIL.Cog.Applications.ViewModels
{
	public class TaskAreaCommandGroupViewModel : TaskAreaViewModelBase
	{
		private readonly ReadOnlyList<TaskAreaCommandViewModel> _commands;
		private TaskAreaCommandViewModel _selectedCommand;

		public TaskAreaCommandGroupViewModel(params TaskAreaCommandViewModel[] commands)
			: this(null, commands)
		{
			
		} 

		public TaskAreaCommandGroupViewModel(string displayName, params TaskAreaCommandViewModel[] commands)
			: base(displayName)
		{
			_commands = new ReadOnlyList<TaskAreaCommandViewModel>(commands);
			if (_commands.Count > 0)
				_selectedCommand = _commands[0];
		}

		public ReadOnlyList<TaskAreaCommandViewModel> Commands
		{
			get { return _commands; }
		}

		public TaskAreaCommandViewModel SelectedCommand
		{
			get { return _selectedCommand; }
			set { Set(() => SelectedCommand, ref _selectedCommand, value); }
		}
	}
}
