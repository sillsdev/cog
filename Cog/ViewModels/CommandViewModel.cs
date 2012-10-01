using System.Windows.Input;
using GalaSoft.MvvmLight;

namespace SIL.Cog.ViewModels
{
	public class CommandViewModel : ViewModelBase
	{
		private readonly string _displayName;
		private readonly ICommand _command;

		public CommandViewModel(string displayName, ICommand command)
		{
			_displayName = displayName;
			_command = command;
		}

		public ICommand Command
		{
			get { return _command; }
		}

		public string DisplayName
		{
			get { return _displayName; }
		}

		public override string ToString()
		{
			return _displayName;
		}
	}
}
