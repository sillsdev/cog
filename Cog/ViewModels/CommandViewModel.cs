using System.Windows.Input;

namespace SIL.Cog.ViewModels
{
	public class CommandViewModel : CogViewModelBase
	{
		private readonly ICommand _command;

		public CommandViewModel(string displayName, ICommand command)
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
