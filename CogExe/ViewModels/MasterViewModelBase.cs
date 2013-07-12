using System;
using GalaSoft.MvvmLight.Messaging;
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

		public override void Initialize(CogProject project)
		{
			foreach (InitializableViewModelBase vm in _views)
				vm.Initialize(project);
		}

		public override bool SwitchView(Type viewType, IReadOnlyList<object> models)
		{
			foreach (InitializableViewModelBase view in _views)
			{
				if (view.SwitchView(viewType, models))
				{
					Set(() => CurrentView, ref _currentView, view);
					return true;
				}
			}

			return false;
		}
	}
}
