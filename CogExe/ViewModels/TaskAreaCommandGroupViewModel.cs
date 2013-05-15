namespace SIL.Cog.ViewModels
{
	public class TaskAreaCommandGroupViewModel : TaskAreaCommandsViewModel
	{
		private CogViewModelBase _currentCommand;

		public TaskAreaCommandGroupViewModel(string displayName, params CommandViewModel[] commands)
			: base(displayName, commands)
		{
			if (Commands.Count > 0)
				_currentCommand = Commands[0];
		}

		public CogViewModelBase CurrentCommand
		{
			get { return _currentCommand; }
			set { Set(() => CurrentCommand, ref _currentCommand, value); }
		}
	}
}
