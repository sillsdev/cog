using System.Collections.ObjectModel;

namespace SIL.Cog.ViewModels
{
	public abstract class WorkspaceViewModel : CogViewModel
	{
		private readonly ObservableCollection<TaskAreaViewModel> _taskAreas; 

		protected WorkspaceViewModel(string displayName)
			: base(displayName)
		{
			_taskAreas = new ObservableCollection<TaskAreaViewModel>();
		}

		public ObservableCollection<TaskAreaViewModel> TaskAreas
		{
			get { return _taskAreas; }
		}
	}
}
