using GalaSoft.MvvmLight;
using SIL.Collections;

namespace SIL.Cog.Applications.ViewModels
{
	public abstract class WorkspaceViewModelBase : ViewModelBase
	{
		private readonly BindableList<TaskAreaViewModelBase> _taskAreas;
		private readonly string _displayName;

		protected WorkspaceViewModelBase(string displayName)
		{
			_displayName = displayName;
			_taskAreas = new BindableList<TaskAreaViewModelBase>();
		}

		public string DisplayName
		{
			get { return _displayName; }
		}

		public ObservableList<TaskAreaViewModelBase> TaskAreas
		{
			get { return _taskAreas; }
		}
	}
}
