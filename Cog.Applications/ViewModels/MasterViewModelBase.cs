using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using SIL.Collections;

namespace SIL.Cog.Applications.ViewModels
{
	public abstract class MasterViewModelBase : ViewModelBase
	{
		private ViewModelBase _currentView;
		private readonly ReadOnlyList<ViewModelBase> _views;
		private string _displayName;

		protected MasterViewModelBase(string displayName, params ViewModelBase[] views)
		{
			_displayName = displayName;
			_views = new ReadOnlyList<ViewModelBase>(views);
			if (_views.Count > 0)
				CurrentView = _views[0];
		}

		public string DisplayName
		{
			get { return _displayName; }
			set { Set(() => DisplayName, ref _displayName, value); }
		}

		public ReadOnlyList<ViewModelBase> Views
		{
			get { return _views; }
		}

		public ViewModelBase CurrentView
		{
			get { return _currentView; }
			set
			{
				object oldView = _currentView;
				if (Set(() => CurrentView, ref _currentView, value))
				{
					var cv = _currentView as MasterViewModelBase;
					if (cv != null)
					{
						if (cv.Views.Count > 0)
							cv.CurrentView = cv.Views[0];
					}
					else
					{
						Messenger.Default.Send(new ViewChangedMessage(oldView, _currentView));
					}
				}
			}
		}

		protected bool SwitchView(Type viewType)
		{
			foreach (ViewModelBase view in _views)
			{
				var masterVM = view as MasterViewModelBase;
				if (view.GetType() == viewType)
				{
					ViewModelBase oldView = _currentView;
					Set(() => CurrentView, ref _currentView, view);
					Messenger.Default.Send(new ViewChangedMessage(oldView, _currentView));
					return true;
				}
				if (masterVM != null && masterVM.SwitchView(viewType))
				{
					Set(() => CurrentView, ref _currentView, view);
					return true;
				}
			}

			return false;
		}
	}
}
