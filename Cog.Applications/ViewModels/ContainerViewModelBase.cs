using System;
using SIL.Collections;

namespace SIL.Cog.Applications.ViewModels
{
	public abstract class ContainerViewModelBase : ContainerChildViewModelBase
	{
		private ContainerChildViewModelBase _currentView;
		private readonly ReadOnlyList<ContainerChildViewModelBase> _views;

		protected ContainerViewModelBase(string displayName, params ContainerChildViewModelBase[] views)
			: base(displayName)
		{
			_views = new ReadOnlyList<ContainerChildViewModelBase>(views);
		}

		public ReadOnlyList<ContainerChildViewModelBase> Views
		{
			get { return _views; }
		}

		public ContainerChildViewModelBase CurrentView
		{
			get { return _currentView; }
			set
			{
				ContainerChildViewModelBase oldView = _currentView;
				if (Set(() => CurrentView, ref _currentView, value))
				{
					OnCurrentViewChanged(oldView, _currentView);
					var newMaster = _currentView as ContainerViewModelBase;
					if (newMaster != null)
					{
						if (newMaster.Views.Count > 0)
							newMaster.CurrentView = newMaster.Views[0];
					}
				}
			}
		}

		protected virtual void OnCurrentViewChanged(ContainerChildViewModelBase oldView, ContainerChildViewModelBase newView)
		{
			if (oldView != null)
				oldView.IsCurrent = false;
			newView.IsCurrent = true;
		}

		protected bool SwitchView(Type viewType)
		{
			ContainerChildViewModelBase oldView = _currentView;
			foreach (ContainerChildViewModelBase view in _views)
			{
				var masterVM = view as ContainerViewModelBase;
				if (view.GetType() == viewType)
				{
					if (Set(() => CurrentView, ref _currentView, view))
						OnCurrentViewChanged(oldView, _currentView);
					return true;
				}
				if (masterVM != null && masterVM.SwitchView(viewType))
				{
					if (Set(() => CurrentView, ref _currentView, view))
						OnCurrentViewChanged(oldView, _currentView);
					return true;
				}
			}

			return false;
		}

		protected override void OnIsCurrentChanged()
		{
			if (!IsCurrent)
			{
				foreach (ContainerChildViewModelBase view in _views)
					view.IsCurrent = false;
			}
		}
	}
}
