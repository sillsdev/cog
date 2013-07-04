using System;
using SIL.Collections;

namespace SIL.Cog.ViewModels
{
	public abstract class MasterViewModelBase : InitializableViewModelBase
	{
		private InitializableViewModelBase _currentView;
		private readonly ReadOnlyList<InitializableViewModelBase> _views;

		protected MasterViewModelBase(string displayName, params InitializableViewModelBase[] views)
			: base(displayName)
		{
			_views = new ReadOnlyList<InitializableViewModelBase>(views);
			if (_views.Count > 0)
				CurrentView = _views[0];
			foreach (InitializableViewModelBase view in _views)
				view.PropertyChanged += ChildPropertyChanged;
		}

		public override void AcceptChanges()
		{
			base.AcceptChanges();
			ChildrenAcceptChanges(_views);
		}

		public ReadOnlyList<InitializableViewModelBase> Views
		{
			get { return _views; }
		}

		public InitializableViewModelBase CurrentView
		{
			get { return _currentView; }
			set
			{
				Set(() => CurrentView, ref _currentView, value);
				var cv = _currentView as MasterViewModelBase;
				if (cv != null && cv.Views.Count > 0)
					cv.CurrentView = cv.Views[0];
			}
		}

		public override void Initialize(CogProject project)
		{
			foreach (InitializableViewModelBase vm in _views)
				vm.Initialize(project);
		}

		public override bool SwitchView(Type viewType, object model)
		{
			foreach (InitializableViewModelBase view in _views)
			{
				if (view.SwitchView(viewType, model))
				{
					Set(() => CurrentView, ref _currentView, view);
					return true;
				}
			}

			return false;
		}
	}
}
