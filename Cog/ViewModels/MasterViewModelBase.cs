using System;
using System.Collections.ObjectModel;

namespace SIL.Cog.ViewModels
{
	public abstract class MasterViewModelBase : InitializableCogViewModelBase
	{
		private InitializableCogViewModelBase _currentView;
		private readonly ReadOnlyCollection<InitializableCogViewModelBase> _views; 

		protected MasterViewModelBase(string displayName, params InitializableCogViewModelBase[] views)
			: base(displayName)
		{
			_views = new ReadOnlyCollection<InitializableCogViewModelBase>(views);
			if (_views.Count > 0)
				CurrentView = _views[0];
		}

		public ReadOnlyCollection<InitializableCogViewModelBase> Views
		{
			get { return _views; }
		}

		public InitializableCogViewModelBase CurrentView
		{
			get { return _currentView; }
			set { Set("CurrentView", ref _currentView, value); }
		}

		public override void Initialize(CogProject project)
		{
			foreach (InitializableCogViewModelBase vm in _views)
				vm.Initialize(project);
		}

		public override bool SwitchView(Type viewType, object model)
		{
			foreach (InitializableCogViewModelBase view in _views)
			{
				if (view.SwitchView(viewType, model))
				{
					CurrentView = view;
					return true;
				}
			}

			return false;
		}
	}
}
