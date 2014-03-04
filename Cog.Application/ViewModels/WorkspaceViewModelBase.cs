using SIL.Cog.Application.Collections;
using SIL.Collections;

namespace SIL.Cog.Application.ViewModels
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
