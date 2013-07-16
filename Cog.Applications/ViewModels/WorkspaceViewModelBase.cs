using System;
using SIL.Collections;

namespace SIL.Cog.Applications.ViewModels
{
	public abstract class WorkspaceViewModelBase : InitializableViewModelBase
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

		public override bool SwitchView(Type viewType, IReadOnlyList<object> models)
		{
			return viewType == GetType();
		}
	}
}
