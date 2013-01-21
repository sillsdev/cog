namespace SIL.Cog.ViewModels
{
	public class TaskAreaGroupViewModel : TaskAreaViewModel
	{
		private CogViewModelBase _currentCommand;

		public TaskAreaGroupViewModel(string displayName, params CommandViewModel[] commands)
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
