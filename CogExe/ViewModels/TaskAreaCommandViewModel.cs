using System.Windows.Input;

namespace SIL.Cog.ViewModels
{
	public class TaskAreaCommandViewModel : TaskAreaViewModelBase
	{
		private readonly ICommand _command;

		public TaskAreaCommandViewModel(string displayName, ICommand command)
			: base(displayName)
		{
			_command = command;
		}

		public ICommand Command
		{
			get { return _command; }
		}
	}
}
