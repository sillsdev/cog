using System;
using SIL.Collections;

namespace SIL.Cog.ViewModels
{
	public abstract class WorkspaceViewModelBase : InitializableViewModelBase
	{
		private readonly BindableList<CogViewModelBase> _taskAreas; 

		protected WorkspaceViewModelBase(string displayName)
			: base(displayName)
		{
			_taskAreas = new BindableList<CogViewModelBase>();
		}

		public ObservableList<CogViewModelBase> TaskAreas
		{
			get { return _taskAreas; }
		}

		public override bool SwitchView(Type viewType, IReadOnlyList<object> models)
		{
			return viewType == GetType();
		}
	}
}
