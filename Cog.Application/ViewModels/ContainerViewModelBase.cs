using System;
using SIL.ObjectModel;

namespace SIL.Cog.Application.ViewModels
{
	public abstract class ContainerViewModelBase : ContainerChildViewModelBase
	{
		private ContainerChildViewModelBase _selectedView;
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

		public ContainerChildViewModelBase SelectedView
		{
			get { return _selectedView; }
			set
			{
				ContainerChildViewModelBase oldView = _selectedView;
				Set(() => SelectedView, ref _selectedView, value);
				OnSelectedViewChanged(oldView, _selectedView);
				var newMaster = _selectedView as ContainerViewModelBase;
				if (newMaster != null)
				{
					if (newMaster.Views.Count > 0)
						newMaster.SelectedView = newMaster.Views[0];
				}
			}
		}

		protected virtual void OnSelectedViewChanged(ContainerChildViewModelBase oldView, ContainerChildViewModelBase newView)
		{
			if (oldView != null)
				oldView.IsSelected = false;
			newView.IsSelected = true;
		}

		protected bool SwitchView(Type viewType)
		{
			ContainerChildViewModelBase oldView = _selectedView;
			foreach (ContainerChildViewModelBase view in _views)
			{
				var masterVM = view as ContainerViewModelBase;
				if (view.GetType() == viewType)
				{
					if (Set(() => SelectedView, ref _selectedView, view))
						OnSelectedViewChanged(oldView, _selectedView);
					return true;
				}
				if (masterVM != null && masterVM.SwitchView(viewType))
				{
					if (Set(() => SelectedView, ref _selectedView, view))
						OnSelectedViewChanged(oldView, _selectedView);
					return true;
				}
			}

			return false;
		}

		protected override void OnIsSelectedChanged()
		{
			if (!IsSelected)
			{
				foreach (ContainerChildViewModelBase view in _views)
					view.IsSelected = false;
			}
		}
	}
}
