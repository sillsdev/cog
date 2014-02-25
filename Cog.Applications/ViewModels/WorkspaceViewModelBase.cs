using SIL.Cog.Applications.Collections;
using SIL.Collections;

namespace SIL.Cog.Applications.ViewModels
{
	public abstract class WorkspaceViewModelBase : ContainerChildViewModelBase
	{
		private readonly BindableList<TaskAreaViewModelBase> _taskAreas;

		protected WorkspaceViewModelBase(string displayName)
			: base(displayName)
		{
			_taskAreas = new BindableList<TaskAreaViewModelBase>();
		}

		public ObservableList<TaskAreaViewModelBase> TaskAreas
		{
			get { return _taskAreas; }
		}
	}
}
