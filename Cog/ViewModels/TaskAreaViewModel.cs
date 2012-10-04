using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SIL.Cog.ViewModels
{
	public class TaskAreaViewModel : CogViewModelBase
	{
		private readonly ReadOnlyCollection<CommandViewModel> _tasks;

		public TaskAreaViewModel(string displayName, IEnumerable<CommandViewModel> tasks)
			: base(displayName)
		{
			_tasks = new ReadOnlyCollection<CommandViewModel>(tasks.ToArray());
		}

		public ReadOnlyCollection<CommandViewModel> Tasks
		{
			get { return _tasks; }
		}
	}
}
