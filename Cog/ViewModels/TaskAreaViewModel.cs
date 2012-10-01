using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;

namespace SIL.Cog.ViewModels
{
	public class TaskAreaViewModel : ViewModelBase
	{
		private readonly ReadOnlyCollection<CommandViewModel> _tasks;
		private readonly string _displayName;

		public TaskAreaViewModel(string displayName, IEnumerable<CommandViewModel> tasks)
		{
			_displayName = displayName;
			_tasks = new ReadOnlyCollection<CommandViewModel>(tasks.ToArray());
		}

		public string DisplayName
		{
			get { return _displayName; }
		}

		public ReadOnlyCollection<CommandViewModel> Tasks
		{
			get { return _tasks; }
		}
	}
}
