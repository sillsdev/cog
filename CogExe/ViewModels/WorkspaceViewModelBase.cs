using System;
using SIL.Collections;

namespace SIL.Cog.ViewModels
{
	public abstract class WorkspaceViewModelBase : InitializableViewModelBase
	{
		private readonly ObservableList<CogViewModelBase> _taskAreas; 

		protected WorkspaceViewModelBase(string displayName)
			: base(displayName)
		{
			_taskAreas = new ObservableList<CogViewModelBase>();
		}

		public ObservableList<CogViewModelBase> TaskAreas
		{
			get { return _taskAreas; }
		}

		public override bool SwitchView(Type viewType, object model)
		{
			return viewType == GetType();
		}
	}
}
